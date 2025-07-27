using System.Text.Json.Serialization;

namespace AI.GithubCopilot.Infrastructure.Models;

/// <summary>
/// Prompt filter result for input validation
/// </summary>
public record PromptFilterResult(
    [property: JsonPropertyName("prompt_index")] int PromptIndex,
    [property: JsonPropertyName("content_filter_results")] ContentFilterResults ContentFilterResults
);
