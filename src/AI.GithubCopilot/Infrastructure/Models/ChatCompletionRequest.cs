using System.Text.Json.Serialization;

namespace AI.GithubCopilot.Infrastructure.Models;

/// <summary>
/// Main request body for GitHub Copilot Chat Completions API
/// </summary>
public record ChatCompletionRequest(
    [property: JsonPropertyName("messages")] IReadOnlyList<ChatMessage> Messages,
    [property: JsonPropertyName("model")] string? Model = null,
    [property: JsonPropertyName("temperature")] double? Temperature = null,
    [property: JsonPropertyName("max_tokens")] int? MaxTokens = null,
    [property: JsonPropertyName("stream")] bool Stream = false,
    [property: JsonPropertyName("top_p")] double? TopP = null,
    [property: JsonPropertyName("frequency_penalty")] double? FrequencyPenalty = null,
    [property: JsonPropertyName("presence_penalty")] double? PresencePenalty = null,
    [property: JsonPropertyName("stop")] IReadOnlyList<string>? Stop = null,
    [property: JsonPropertyName("user")] string? User = null,
    [property: JsonPropertyName("n")] int? N = null,
    
    // Additional properties from GitHub Copilot API
    [property: JsonPropertyName("logit_bias")] Dictionary<string, double>? LogitBias = null,
    [property: JsonPropertyName("logprobs")] bool? LogProbs = null,
    [property: JsonPropertyName("top_logprobs")] int? TopLogProbs = null,
    [property: JsonPropertyName("response_format")] ResponseFormat? ResponseFormat = null,
    [property: JsonPropertyName("seed")] int? Seed = null,
    [property: JsonPropertyName("tools")] IReadOnlyList<Tool>? Tools = null,
    [property: JsonPropertyName("tool_choice")] ToolChoice? ToolChoice = null,
    [property: JsonPropertyName("parallel_tool_calls")] bool? ParallelToolCalls = null
);