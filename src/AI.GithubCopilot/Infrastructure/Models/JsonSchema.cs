using System.Text.Json.Serialization;

namespace AI.GithubCopilot.Infrastructure.Models;

/// <summary>
/// JSON Schema definition for structured response format
/// </summary>
public record JsonSchema(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("description")] string? Description = null,
    [property: JsonPropertyName("schema")] object? Schema = null,
    [property: JsonPropertyName("strict")] bool? Strict = null
);
