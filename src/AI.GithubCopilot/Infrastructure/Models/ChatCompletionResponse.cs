using System.Text.Json.Serialization;

namespace AI.GithubCopilot.Infrastructure.Models;

/// <summary>
/// Main response from GitHub Copilot Chat Completions API
/// </summary>
public record ChatCompletionResponse(
    [property: JsonPropertyName("choices")] IReadOnlyList<CompletionChoice>? Choices,
    [property: JsonPropertyName("usage")] TokenUsage? Usage,
    [property: JsonPropertyName("copilot_references")] IReadOnlyList<CopilotReferenceWithMetadata>? CopilotReferences,
    [property: JsonPropertyName("finish_reason")] string? FinishReason,
    [property: JsonPropertyName("done_reason")] string? DoneReason
);