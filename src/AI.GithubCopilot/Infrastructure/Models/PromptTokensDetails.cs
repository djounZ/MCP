using System.Text.Json.Serialization;

namespace AI.GithubCopilot.Infrastructure.Models;

/// <summary>
/// Detailed prompt token usage information
/// </summary>
public record PromptTokensDetails(
    [property: JsonPropertyName("cached_tokens")] int CachedTokens
);
