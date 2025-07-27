using System.Text.Json.Serialization;

namespace AI.GithubCopilot.Infrastructure.Models;

/// <summary>
/// Usage statistics for the completion
/// </summary>
public record Usage(
    [property: JsonPropertyName("completion_tokens")] int CompletionTokens,
    [property: JsonPropertyName("prompt_tokens")] int PromptTokens,
    [property: JsonPropertyName("total_tokens")] int TotalTokens,
    [property: JsonPropertyName("completion_tokens_details")] CompletionTokensDetails? CompletionTokensDetails = null,
    [property: JsonPropertyName("prompt_tokens_details")] PromptTokensDetails? PromptTokensDetails = null
);