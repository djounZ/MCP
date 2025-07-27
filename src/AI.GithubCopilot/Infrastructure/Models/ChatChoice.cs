using System.Text.Json.Serialization;

namespace AI.GithubCopilot.Infrastructure.Models;

/// <summary>
/// Individual choice in a chat completion response
/// </summary>
public record ChatChoice(
    [property: JsonPropertyName("index")] int Index,
    [property: JsonPropertyName("message")] ChatMessage? Message = null,
    [property: JsonPropertyName("delta")] ChatDelta? Delta = null,
    [property: JsonPropertyName("finish_reason")] string? FinishReason = null,
    [property: JsonPropertyName("logprobs")] LogProbabilityInfo? LogProbs = null,
    [property: JsonPropertyName("content_filter_results")] ContentFilterResults? ContentFilterResults = null,
    [property: JsonPropertyName("content_filter_offsets")] ContentFilterOffsets? ContentFilterOffsets = null
);
