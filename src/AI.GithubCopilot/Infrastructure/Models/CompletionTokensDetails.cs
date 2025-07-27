using System.Text.Json.Serialization;

namespace AI.GithubCopilot.Infrastructure.Models;

/// <summary>
/// Detailed completion token usage information
/// </summary>
public record CompletionTokensDetails(
    [property: JsonPropertyName("reasoning_tokens")] int? ReasoningTokens = null,
    [property: JsonPropertyName("accepted_prediction_tokens")] int? AcceptedPredictionTokens = null,
    [property: JsonPropertyName("rejected_prediction_tokens")] int? RejectedPredictionTokens = null
);
