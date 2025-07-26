using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using MCP.Infrastructure.Models;
using Microsoft.Extensions.Logging.Abstractions;

namespace MCP.Infrastructure.Services;

/// <summary>
/// Configuration for the GitHub Copilot Chat client
/// </summary>
public record CopilotChatConfig(
    string EditorName = "Neovim",
    string EditorVersion = "0.10.2",
    string PluginName = "CopilotChat.nvim",
    string PluginVersion = "*");


/// <summary>
/// Represents a cached Copilot authentication token
/// </summary>
internal record CachedToken(string Token, DateTimeOffset ExpiresAt);

/// <summary>
/// Response from the GitHub Copilot token endpoint
/// </summary>
internal record TokenResponse(
    [property: JsonPropertyName("token")] string Token,
    [property: JsonPropertyName("expires_at")] long ExpiresAt
);


/// <summary>
/// Represents an available Copilot model
/// </summary>
public record CopilotModel(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("capabilities")] ModelCapabilities Capabilities,
    [property: JsonPropertyName("version")] string Version,
    [property: JsonPropertyName("policy")] ModelPolicy? Policy = null
);

/// <summary>
/// Model capabilities and limits
/// </summary>
public record ModelCapabilities(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("tokenizer")] string Tokenizer,
    [property: JsonPropertyName("limits")] ModelLimits Limits
);

/// <summary>
/// Model token limits
/// </summary>
public record ModelLimits(
    [property: JsonPropertyName("max_prompt_tokens")] int MaxPromptTokens,
    [property: JsonPropertyName("max_output_tokens")] int MaxOutputTokens
);

/// <summary>
/// Model policy information
/// </summary>
public record ModelPolicy(
    [property: JsonPropertyName("state")] string State,
    [property: JsonPropertyName("updated_at")] string? UpdatedAt = null
);

/// <summary>
/// Response from the models API endpoint
/// </summary>
internal record ModelsResponse(
    [property: JsonPropertyName("data")] CopilotModel[] Data
);


/// <summary>
/// Processed model information for easier consumption
/// </summary>
public record ProcessedModel(
    string Id,
    string Name,
    string Tokenizer,
    int MaxInputTokens,
    int MaxOutputTokens,
    bool PolicyEnabled,
    string Version
);
/// <summary>
/// Client for interacting with the GitHub Copilot Chat Completions API
/// </summary>
public class CopilotChatClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly CopilotChatConfig _config;
    private readonly SemaphoreSlim _tokenSemaphore = new(1, 1);
    private CachedToken? _cachedToken;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public CopilotChatClient(CopilotChatConfig? config = null, HttpClient? httpClient = null)
    {
        _config = config ?? new CopilotChatConfig();
        _httpClient = httpClient ?? new HttpClient();

        // Set default headers
        _httpClient.DefaultRequestHeaders.Add("User-Agent", $"{_config.EditorName}/{_config.EditorVersion}");
    }

    /// <summary>
    /// Gets the GitHub OAuth token from environment or config files
    /// </summary>
    private string GetGitHubToken()
    {
        return EnvHelpers
            .GetEnvironmentVariableAsync<GithubAccessTokenResponse>(nameof(GithubAccessTokenResponse),
                NullLogger.Instance).ConfigureAwait(false).GetAwaiter().GetResult()?.AccessToken ?? string.Empty;
    }

    /// <summary>
    /// Gets a valid Copilot Bearer token, refreshing if necessary
    /// </summary>
    private async Task<string> GetCopilotTokenAsync(CancellationToken cancellationToken = default)
    {
        await _tokenSemaphore.WaitAsync(cancellationToken);
        try
        {
            // Check if cached token is still valid
            if (_cachedToken != null && _cachedToken.ExpiresAt > DateTimeOffset.UtcNow.AddMinutes(5))
            {
                return _cachedToken.Token;
            }

            // Get GitHub token and exchange for Copilot token
            var githubToken = GetGitHubToken();

            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/copilot_internal/v2/token");
            request.Headers.Authorization = new AuthenticationHeaderValue("Token", githubToken);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseContent, JsonOptions)
                ?? throw new InvalidOperationException("Invalid token response");

            var expiresAt = DateTimeOffset.FromUnixTimeSeconds(tokenResponse.ExpiresAt);
            _cachedToken = new CachedToken(tokenResponse.Token, expiresAt);

            return _cachedToken.Token;
        }
        finally
        {
            _tokenSemaphore.Release();
        }
    }

    /// <summary>
    /// Gets the appropriate headers for API requests
    /// </summary>
    private async Task<Dictionary<string, string>> GetHeadersAsync(CancellationToken cancellationToken = default)
    {
        var token = await GetCopilotTokenAsync(cancellationToken);

        return new Dictionary<string, string>
        {
            ["Authorization"] = $"Bearer {token}",
            ["Editor-Version"] = $"{_config.EditorName}/{_config.EditorVersion}",
            ["Editor-Plugin-Version"] = $"{_config.PluginName}/{_config.PluginVersion}",
            ["Copilot-Integration-Id"] = "vscode-chat"
        };
    }

    /// <summary>
    /// Sends a chat completion request and returns the complete response
    /// </summary>
    public async Task<ProcessedChatResponse> GetChatCompletionAsync(
        ChatCompletionRequest request,
        CancellationToken cancellationToken = default)
    {
        request = request with { Stream = false };
        var headers = await GetHeadersAsync(cancellationToken);
        var json = JsonSerializer.Serialize(request, JsonOptions);

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.githubcopilot.com/chat/completions")
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        foreach (var (key, value) in headers)
        {
            httpRequest.Headers.Add(key, value);
        }

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var chatResponse = JsonSerializer.Deserialize<ChatCompletionResponse>(responseContent, JsonOptions)
            ?? throw new InvalidOperationException("Invalid response format");

        return ChatResponseProcessor.ProcessResponse(chatResponse);
    }

    /// <summary>
    /// Sends a chat completion request with streaming support
    /// </summary>
    public async IAsyncEnumerable<ProcessedChatResponse> GetChatCompletionStreamAsync(
        ChatCompletionRequest request,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // Ensure streaming is enabled
        var streamingRequest = request with { Stream = true };

        var headers = await GetHeadersAsync(cancellationToken);
        var json = JsonSerializer.Serialize(streamingRequest, JsonOptions);

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.githubcopilot.com/chat/completions")
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        foreach (var (key, value) in headers)
        {
            httpRequest.Headers.Add(key, value);
        }

        var response = await _httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync();

            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("event:") || line.StartsWith(":"))
                continue;

            var jsonLine = line.StartsWith("data: ") ? line[6..] : line;

            if (jsonLine == "[DONE]")
                break;

            ChatCompletionResponse? chatResponse = null;
            try
            {
                chatResponse = JsonSerializer.Deserialize<ChatCompletionResponse>(jsonLine, JsonOptions);
            }
            catch (JsonException)
            {
                // Skip invalid JSON lines
                continue;
            }

            if (chatResponse != null)
            {
                yield return ChatResponseProcessor.ProcessResponse(chatResponse);
            }
        }
    }

    /// <summary>
    /// Convenience method to send a simple chat message
    /// </summary>
    public async Task<ProcessedChatResponse> SendMessageAsync(
        string message,
        string model = "gpt-4.1",
        string? systemPrompt = null,
        double? temperature = null,
        CancellationToken cancellationToken = default)
    {
        var builder = new ChatCompletionRequestBuilder()
            .WithModel(model)
            .AddUserMessage(message);

        if (!string.IsNullOrEmpty(systemPrompt))
        {
            builder.AddSystemMessage(systemPrompt);
        }

        if (temperature.HasValue)
        {
            builder.WithTemperature(temperature.Value);
        }

        var request = builder.Build();
        return await GetChatCompletionAsync(request, cancellationToken);
    }

    /// <summary>
    /// Convenience method to send a simple chat message with streaming
    /// </summary>
    public IAsyncEnumerable<ProcessedChatResponse> SendMessageStreamAsync(
        string message,
        string model = "gpt-4.1",
        string? systemPrompt = null,
        double? temperature = null,
        CancellationToken cancellationToken = default)
    {
        var builder = new ChatCompletionRequestBuilder()
            .WithModel(model)
            .AddUserMessage(message)
            .WithStreaming(true);

        if (!string.IsNullOrEmpty(systemPrompt))
        {
            builder.AddSystemMessage(systemPrompt);
        }

        if (temperature.HasValue)
        {
            builder.WithTemperature(temperature.Value);
        }

        var request = builder.Build();
        return GetChatCompletionStreamAsync(request, cancellationToken);
    }

    /// <summary>
    /// Gets the list of available Copilot models
    /// </summary>
    public async Task<IReadOnlyList<ProcessedModel>> GetAvailableModelsAsync(
        CancellationToken cancellationToken = default)
    {
        var headers = await GetHeadersAsync(cancellationToken);

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, "https://api.githubcopilot.com/models");

        foreach (var (key, value) in headers)
        {
            httpRequest.Headers.Add(key, value);
        }

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var modelsResponse = JsonSerializer.Deserialize<ModelsResponse>(responseContent, JsonOptions)
            ?? throw new InvalidOperationException("Invalid models response format");

        // Filter models for chat type and non-paygo
        var filteredModels = modelsResponse.Data
            .Where(model =>
                model.Capabilities.Type == "chat" &&
                !model.Id.EndsWith("paygo", StringComparison.OrdinalIgnoreCase))
            .ToList();

        // Group by name and get latest version
        var latestVersionModels = filteredModels
            .GroupBy(model => model.Name)
            .Select(group => group.OrderByDescending(m => m.Version).First())
            .ToList();

        // Check and enable policies for models that don't have them
        var modelsWithPolicies = new List<CopilotModel>();
        foreach (var model in latestVersionModels)
        {
            modelsWithPolicies.Add(model);
        }

        // Convert to processed models and filter by enabled policy
        var processedModels = modelsWithPolicies
            .Select(model => new ProcessedModel(
                Id: model.Id,
                Name: model.Name,
                Tokenizer: model.Capabilities.Tokenizer,
                MaxInputTokens: model.Capabilities.Limits.MaxPromptTokens,
                MaxOutputTokens: model.Capabilities.Limits.MaxOutputTokens,
                PolicyEnabled: model.Policy?.State == "enabled" || model.Policy == null,
                Version: model.Version
            ))
            .Where(model => model.PolicyEnabled) // Only include enabled models
            .OrderBy(model => model.Name)
            .ToList();

        return processedModels;
    }

    /// <summary>
    /// Gets the policy status for a specific model
    /// </summary>
    public async Task<ModelPolicy?> GetModelPolicyAsync(
        string modelId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var headers = await GetHeadersAsync(cancellationToken);

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"https://api.githubcopilot.com/models/{modelId}/policy");

            foreach (var (key, value) in headers)
            {
                httpRequest.Headers.Add(key, value);
            }

            var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var policyResponse = JsonSerializer.Deserialize<ModelPolicy>(responseContent, JsonOptions);
                return policyResponse != null ? new ModelPolicy(policyResponse.State) : null;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Checks if a specific model is available and enabled
    /// </summary>
    public async Task<bool> IsModelAvailableAsync(
        string modelId,
        CancellationToken cancellationToken = default)
    {
        var models = await GetAvailableModelsAsync(cancellationToken);
        return models.Any(m => m.Id == modelId);
    }

    /// <summary>
    /// Gets detailed information about a specific model
    /// </summary>
    public async Task<ProcessedModel?> GetModelInfoAsync(
        string modelId,
        CancellationToken cancellationToken = default)
    {
        var models = await GetAvailableModelsAsync(cancellationToken);
        return models.FirstOrDefault(m => m.Id == modelId);
    }

    public void Dispose()
    {
        _tokenSemaphore?.Dispose();
        _httpClient?.Dispose();
    }
}

