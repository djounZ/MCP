using System.Text.Json.Serialization;

namespace AI.GithubCopilot.Infrastructure.Models;

/// <summary>
/// Individual choice in the completion response
/// </summary>
public record CompletionChoice(
    [property: JsonPropertyName("index")] int Index,
    [property: JsonPropertyName("delta")] MessageDelta? Delta = null,
    [property: JsonPropertyName("finish_reason")] string? FinishReason = null,
    [property: JsonPropertyName("content_filter_results")] ContentFilterResults? ContentFilterResults = null,
    [property: JsonPropertyName("content_filter_offsets")] ContentFilterOffsets? ContentFilterOffsets = null,
    [property: JsonPropertyName("message")] ResponseMessage? Message = null,
    [property: JsonPropertyName("done_reason")] string? DoneReason = null,
    [property: JsonPropertyName("usage")] Usage? Usage = null
);