using System.Text.Json.Serialization;

namespace AI.GithubCopilot.Infrastructure.Models;

/// <summary>
/// Main response from GitHub Copilot Chat Completions API
/// </summary>
public record ChatCompletionResponse(
    [property: JsonPropertyName("choices")] IReadOnlyList<ChatChoice> Choices,
    [property: JsonPropertyName("created")] long Created,
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("object")] string Object,
    [property: JsonPropertyName("model")] string? Model = null,
    [property: JsonPropertyName("system_fingerprint")] string? SystemFingerprint = null,
    [property: JsonPropertyName("usage")] Usage? Usage = null,
    [property: JsonPropertyName("prompt_filter_results")] IReadOnlyList<PromptFilterResult>? PromptFilterResults = null
);