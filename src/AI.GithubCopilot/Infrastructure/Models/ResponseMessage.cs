using System.Text.Json.Serialization;

namespace AI.GithubCopilot.Infrastructure.Models;

/// <summary>
/// Complete message for non-streaming responses
/// </summary>
public record ResponseMessage(
    [property: JsonPropertyName("content")] string? Content,
    [property: JsonPropertyName("role")] string? Role
);