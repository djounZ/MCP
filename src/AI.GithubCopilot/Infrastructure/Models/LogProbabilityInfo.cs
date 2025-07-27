using System.Text.Json.Serialization;

namespace AI.GithubCopilot.Infrastructure.Models;

/// <summary>
/// Log probability information for tokens
/// </summary>
public record LogProbabilityInfo(
    [property: JsonPropertyName("tokens")] IReadOnlyList<string> Tokens,
    [property: JsonPropertyName("token_logprobs")] IReadOnlyList<double> TokenLogProbs,
    [property: JsonPropertyName("top_logprobs")] IReadOnlyList<IReadOnlyDictionary<string, double>> TopLogProbs,
    [property: JsonPropertyName("text_offset")] IReadOnlyList<int> TextOffset
);
