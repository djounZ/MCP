using System.Text.Json.Serialization;

namespace AI.GithubCopilot.Infrastructure.Models;

/// <summary>
/// Delta content for streaming responses
/// </summary>
public record MessageDelta(
    [property: JsonPropertyName("content")] string? Content,
    [property: JsonPropertyName("role")] string? Role
);