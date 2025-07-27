using System.Text.Json.Serialization;

namespace AI.GithubCopilot.Infrastructure.Models;

/// <summary>
/// Represents a message in the chat completion request
/// </summary>
public record ChatMessage(
    [property: JsonPropertyName("role")] string Role,
    [property: JsonPropertyName("content")] MessageContent Content,
    [property: JsonPropertyName("name")] string? Name = null,
    [property: JsonPropertyName("tool_calls")] ToolCall[]? ToolCalls = null,
    [property: JsonPropertyName("tool_call_id")] string? ToolCallId = null
);