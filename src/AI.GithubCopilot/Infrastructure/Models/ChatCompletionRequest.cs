using System.Text.Json.Serialization;

namespace AI.GithubCopilot.Infrastructure.Models;

/// <summary>
/// Main request body for GitHub Copilot Chat Completions API
/// </summary>
public record ChatCompletionRequest(
    [property: JsonPropertyName("messages")] IReadOnlyList<ChatMessage> Messages,
    [property: JsonPropertyName("model")] string Model,
    [property: JsonPropertyName("n")] int? N = null,
    [property: JsonPropertyName("top_p")] double? TopP = null,
    [property: JsonPropertyName("stream")] bool? Stream = null,
    [property: JsonPropertyName("temperature")] double? Temperature = null,
    [property: JsonPropertyName("max_tokens")] int? MaxTokens = null
);