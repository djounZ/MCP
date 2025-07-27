using System.Text.Json.Serialization;

namespace AI.GithubCopilot.Infrastructure.Models;

/// <summary>
/// Represents a reference returned by Copilot
/// </summary>
public record CopilotReference(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("url")] string Url
);