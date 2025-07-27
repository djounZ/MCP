using System.Text.Json.Serialization;

namespace AI.GithubCopilot.Infrastructure.Models;

/// <summary>
/// Individual content filter result for a specific category
/// </summary>
public record ContentFilterResult(
    [property: JsonPropertyName("filtered")] bool Filtered,
    [property: JsonPropertyName("severity")] string Severity
);
