using System.Text.Json.Serialization;

namespace AI.GithubCopilot.Infrastructure.Models;

/// <summary>
/// Usage statistics for the completion
/// </summary>
public record TokenUsage(
    [property: JsonPropertyName("total_tokens")] int? TotalTokens,
    [property: JsonPropertyName("prompt_tokens")] int? PromptTokens,
    [property: JsonPropertyName("completion_tokens")] int? CompletionTokens
);