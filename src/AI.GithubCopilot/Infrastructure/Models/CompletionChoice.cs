using System.Text.Json.Serialization;

namespace AI.GithubCopilot.Infrastructure.Models;

/// <summary>
/// Individual choice in the completion response
/// </summary>
public record CompletionChoice(
    [property: JsonPropertyName("message")] ResponseMessage? Message,
    [property: JsonPropertyName("delta")] MessageDelta? Delta,
    [property: JsonPropertyName("finish_reason")] string? FinishReason,
    [property: JsonPropertyName("done_reason")] string? DoneReason,
    [property: JsonPropertyName("usage")] TokenUsage? Usage
);