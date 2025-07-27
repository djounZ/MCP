using System.Text.Json.Serialization;

namespace AI.GithubCopilot.Infrastructure.Models;

/// <summary>
/// Function definition for tool calling
/// </summary>
public record FunctionDefinition(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("description")] string? Description = null,
    [property: JsonPropertyName("parameters")] object? Parameters = null,
    [property: JsonPropertyName("strict")] bool? Strict = null
);
