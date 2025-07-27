using System.Text.Json.Serialization;

namespace AI.GithubCopilot.Infrastructure.Models;

/// <summary>
/// Represents a message in the chat completion request
/// </summary>
public record ChatMessage(
    [property: JsonPropertyName("role")] string Role,
    [property: JsonPropertyName("content")] string Content
);