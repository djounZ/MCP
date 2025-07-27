using System.Text.Json.Serialization;

namespace AI.GithubCopilot.Infrastructure.Models;

/// <summary>
/// Content filter results for safety evaluation
/// </summary>
public record ContentFilterResults(
    [property: JsonPropertyName("hate")] ContentFilterResult Hate,
    [property: JsonPropertyName("self_harm")] ContentFilterResult SelfHarm,
    [property: JsonPropertyName("sexual")] ContentFilterResult Sexual,
    [property: JsonPropertyName("violence")] ContentFilterResult Violence
);
