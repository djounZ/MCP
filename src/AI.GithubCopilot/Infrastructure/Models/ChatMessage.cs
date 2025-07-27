using System.Text.Json.Serialization;

namespace AI.GithubCopilot.Infrastructure.Models;

/// <summary>
/// Represents a message in the chat completion request/response
/// </summary>
public record ChatMessage(
    [property: JsonPropertyName("role")] string Role,
    [property: JsonPropertyName("content")] MessageContent? Content = null,
    [property: JsonPropertyName("name")] string? Name = null,
    [property: JsonPropertyName("tool_calls")] IReadOnlyList<ToolCall>? ToolCalls = null,
    [property: JsonPropertyName("tool_call_id")] string? ToolCallId = null,
    [property: JsonPropertyName("refusal")] string? Refusal = null
);