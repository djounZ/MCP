using System.Text.Json.Serialization;

namespace AI.GithubCopilot.Infrastructure.Models;

/// <summary>
/// Delta object for streaming chat completions
/// </summary>
public record ChatDelta(
    [property: JsonPropertyName("content")] string? Content = null,
    [property: JsonPropertyName("role")] string? Role = null,
    [property: JsonPropertyName("tool_calls")] IReadOnlyList<ToolCallDelta>? ToolCalls = null,
    [property: JsonPropertyName("refusal")] string? Refusal = null
);
