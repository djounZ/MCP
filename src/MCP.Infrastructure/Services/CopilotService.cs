

using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using MCP.Domain.Common;
using MCP.Domain.Interfaces;
using MCP.Infrastructure.Constants;
using CopilotServiceOptions = MCP.Infrastructure.Options.CopilotServiceOptions;

namespace MCP.Infrastructure.Services
{
    public sealed class CopilotService : ICopilotService, IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly Timer _tokenRefreshTimer;
        private readonly CopilotServiceOptions _options;
        private readonly ILogger<CopilotService> _logger;
        private string? _token;
        private bool _disposed;

        public CopilotService(HttpClient httpClient, CopilotServiceOptions options, ILogger<CopilotService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Start token refresh timer
            _tokenRefreshTimer = new Timer(async _ =>
            {
                try
                {
                    if (!_disposed)
                    {
                        var tokenResult = await GetTokenInternalAsync();
                        if (tokenResult.IsFailure)
                        {
                            _logger.LogWarning("Token refresh failed: {Error}", tokenResult.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during token refresh timer callback.");
                }
            }, null, TimeSpan.Zero, TimeSpan.FromMinutes(25));
        }

    public async Task<Result<Unit>> SetupAsync()
    {
        try
        {
            var requestData = new
            {
                client_id = _options.ClientId,
                scope = "read:user"
            };

            var headers = new Dictionary<string, string>
            {
                [HeaderKeys.Accept] = ContentTypes.ApplicationJson,
                [HeaderKeys.EditorVersion] = _options.EditorVersion,
                [HeaderKeys.EditorPluginVersion] = _options.EditorPluginVersion,
                [HeaderKeys.ContentType] = ContentTypes.ApplicationJson,
                [HeaderKeys.UserAgent] = _options.UserAgent,
                [HeaderKeys.AcceptEncoding] = _options.AcceptEncoding
            };

            var responseResult = await SendPostRequestAsync(_options.DeviceCodeUrl, requestData, headers);
            if (responseResult.IsFailure)
                return Result.Failure<Unit>($"Failed to get device code: {responseResult.Error}");

            Models.Copilot.CopilotDeviceCodeResponse? deviceCodeResponse;
            try
            {
                deviceCodeResponse = JsonSerializer.Deserialize<Models.Copilot.CopilotDeviceCodeResponse>(responseResult.Value);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse device code response from Copilot.");
                return Result.Failure<Unit>("Failed to parse device code response.");
            }

            if (deviceCodeResponse == null)
            {
                _logger.LogError("Failed to parse device code response from Copilot.");
                return Result.Failure<Unit>("Failed to parse device code response.");
            }

            _logger.LogInformation("Prompting user to authenticate Copilot. VerificationUri: {VerificationUri}, UserCode: {UserCode}", 
                deviceCodeResponse.VerificationUri, deviceCodeResponse.UserCode);
            _logger.LogInformation("Please visit {VerificationUri} and enter code {UserCode} to authenticate", 
                deviceCodeResponse.VerificationUri, deviceCodeResponse.UserCode);

            string? accessToken = null;
            while (accessToken == null)
            {
                await Task.Delay(5000);

                var tokenRequestData = new
                {
                    client_id = _options.ClientId,
                    device_code = deviceCodeResponse.DeviceCode,
                    grant_type = "urn:ietf:params:oauth:grant-type:device_code"
                };

                var tokenResponseResult = await SendPostRequestAsync(_options.AccessTokenUrl, tokenRequestData, headers);
                if (tokenResponseResult.IsFailure)
                    continue; // Keep trying

                try
                {
                    using var tokenResponseJsonDoc = JsonDocument.Parse(tokenResponseResult.Value);
                    var tokenRoot = tokenResponseJsonDoc.RootElement;
                    if (tokenRoot.TryGetProperty("access_token", out var accessTokenProp))
                    {
                        accessToken = accessTokenProp.GetString();
                    }
                }
                catch (JsonException)
                {
                    // Continue trying
                }
            }

            try
            {
                await File.WriteAllTextAsync(".copilot_token", accessToken);
                _logger.LogInformation("Authentication success! Token saved to .copilot_token.");
                return Result.Success();
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "Failed to save token to file.");
                return Result.Failure<Unit>("Failed to save token to file.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during setup.");
            return Result.Failure<Unit>($"Unexpected error during setup: {ex.Message}");
        }
    }

    private async Task<Result<Unit>> GetTokenInternalAsync()
    {
        string accessToken;
        while (true)
        {
            try
            {
                accessToken = await File.ReadAllTextAsync(".copilot_token");
                break;
            }
            catch (FileNotFoundException)
            {
                var setupResult = await SetupAsync();
                if (setupResult.IsFailure)
                    return setupResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading token file");
                return Result.Failure<Unit>($"Error reading token file: {ex.Message}");
            }
        }

        var headers = new Dictionary<string, string>
        {
            [HeaderKeys.Authorization] = $"token {accessToken}",
            [HeaderKeys.EditorVersion] = _options.EditorVersion,
            [HeaderKeys.EditorPluginVersion] = _options.EditorPluginVersion,
            [HeaderKeys.UserAgent] = _options.UserAgent
        };

        var responseResult = await SendGetRequestAsync(_options.TokenUrl, headers);
        if (responseResult.IsFailure)
            return Result.Failure<Unit>($"Failed to get token: {responseResult.Error}");

        try
        {
            using var responseJsonDoc = JsonDocument.Parse(responseResult.Value);
            var responseRoot = responseJsonDoc.RootElement;
            if (responseRoot.TryGetProperty("token", out var tokenProp))
            {
                _token = tokenProp.GetString();
                _logger.LogInformation("Token successfully retrieved from Copilot API.");
                return Result.Success();
            }
            else
            {
                _logger.LogWarning("Token property not found in Copilot API response.");
                return Result.Failure<Unit>("Token property not found in API response");
            }
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse token response");
            return Result.Failure<Unit>("Failed to parse token response");
        }
    }

    public async Task<Result<string>> GetCompletionAsync(string prompt, string language = "python")
    {
        if (_token == null || IsTokenInvalid(_token))
        {
            var tokenResult = await GetTokenInternalAsync();
            if (tokenResult.IsFailure)
                return Result.Failure<string>($"Failed to get token: {tokenResult.Error}");
        }

        var requestData = new
        {
            prompt = prompt,
            suffix = "",
            max_tokens = 1000,
            temperature = 0,
            top_p = 1,
            n = 1,
            stop = new[] { "\n" },
            nwo = "github/copilot.vim",
            stream = true,
            extra = new { language = language }
        };

        var headers = new Dictionary<string, string>
        {
            [HeaderKeys.Authorization] = $"Bearer {_token}"
        };

        try
        {
            var responseResult = await SendPostRequestAsync(_options.CompletionUrl, requestData, headers);
            if (responseResult.IsFailure)
                return Result.Failure<string>($"Failed to get completion: {responseResult.Error}");

            var completion = ParseStreamingResponse(responseResult.Value);
            return Result.Success(completion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while requesting Copilot completion.");
            return Result.Failure<string>($"Error occurred while requesting Copilot completion: {ex.Message}");
        }
    }

    private string ParseStreamingResponse(string responseText)
    {
        var result = new StringBuilder();
        var lines = responseText.Split('\n');

        foreach (var line in lines)
        {
            if (line.StartsWith("data: {"))
            {
                try
                {
                    using var jsonDoc = JsonDocument.Parse(line.Substring(6));
                    var root = jsonDoc.RootElement;
                    if (root.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
                    {
                        var firstChoice = choices[0];
                        if (firstChoice.TryGetProperty("text", out var textProperty))
                        {
                            var completion = textProperty.GetString();
                            if (!string.IsNullOrEmpty(completion))
                            {
                                result.Append(completion);
                            }
                            else
                            {
                                result.Append('\n');
                            }
                        }
                        else
                        {
                            // No text property, likely a completion marker
                            result.Append('\n');
                        }
                    }
                    else
                    {
                        result.Append('\n');
                    }
                }
                catch (JsonException)
                {
                    // Skip invalid JSON lines
                }
            }
        }

        return result.ToString();
    }

    private bool IsTokenInvalid(string token)
    {
        if (string.IsNullOrEmpty(token) || !token.Contains("exp"))
        {
            return true;
        }

        var expValue = ExtractExpValue(token);
        return expValue <= DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    private long ExtractExpValue(string token)
    {
        var pairs = token.Split(';');
        foreach (var pair in pairs)
        {
            var keyValue = pair.Split('=');
            if (keyValue.Length == 2 && keyValue[0].Trim() == "exp")
            {
                if (long.TryParse(keyValue[1].Trim(), out var exp))
                {
                    return exp;
                }
            }
        }
        return 0;
    }

    private async Task<Result<string>> SendPostRequestAsync(string url, object data, Dictionary<string, string> headers)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, url);

            foreach (KeyValuePair<string, string> header in headers)
            {
                if (header.Key == HeaderKeys.ContentType)
                    continue; // This will be set by StringContent
                request.Headers.Add(header.Key, header.Value);
            }

            string json = JsonSerializer.Serialize(data);
            request.Content = new StringContent(json, Encoding.UTF8, ContentTypes.ApplicationJson);

            using HttpResponseMessage response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            
            if (!response.IsSuccessStatusCode)
                return Result.Failure<string>($"HTTP request failed with status code: {response.StatusCode}");

            await using Stream responseStream = await response.Content.ReadAsStreamAsync();
            Stream contentStream = response.Content.Headers.ContentEncoding.Contains(ContentEncodings.Gzip)
                ? new System.IO.Compression.GZipStream(responseStream, System.IO.Compression.CompressionMode.Decompress)
                : responseStream;

            using var reader = new StreamReader(contentStream, Encoding.UTF8);
            string result = await reader.ReadToEndAsync();

            if (contentStream is System.IO.Compression.GZipStream gzipStream)
                await gzipStream.DisposeAsync();

            return Result.Success(result);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request exception in SendPostRequestAsync");
            return Result.Failure<string>($"HTTP request failed: {ex.Message}");
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Request timeout in SendPostRequestAsync");
            return Result.Failure<string>("Request timeout");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in SendPostRequestAsync");
            return Result.Failure<string>($"Unexpected error: {ex.Message}");
        }
    }

    private async Task<Result<string>> SendGetRequestAsync(string url, Dictionary<string, string> headers)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);

            foreach (var header in headers)
            {
                request.Headers.Add(header.Key, header.Value);
            }

            var response = await _httpClient.SendAsync(request);
            
            if (!response.IsSuccessStatusCode)
                return Result.Failure<string>($"HTTP GET request failed with status code: {response.StatusCode}");

            var content = await response.Content.ReadAsStringAsync();
            return Result.Success(content);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request exception in SendGetRequestAsync");
            return Result.Failure<string>($"HTTP request failed: {ex.Message}");
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Request timeout in SendGetRequestAsync");
            return Result.Failure<string>("Request timeout");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in SendGetRequestAsync");
            return Result.Failure<string>($"Unexpected error: {ex.Message}");
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _tokenRefreshTimer?.Dispose();
            _disposed = true;
        }
    }

    }
}
