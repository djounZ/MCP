using System.Text.Json.Serialization;

namespace AI.GithubCopilot.Infrastructure.Models;

/// <summary>
/// Detailed completion token usage information
/// </summary>
public record CompletionTokensDetails(
    [property: JsonPropertyName("accepted_prediction_tokens")] int AcceptedPredictionTokens,
    [property: JsonPropertyName("rejected_prediction_tokens")] int RejectedPredictionTokens
);
