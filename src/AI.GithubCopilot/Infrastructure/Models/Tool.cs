using System.Text.Json.Serialization;

namespace AI.GithubCopilot.Infrastructure.Models;

/// <summary>
/// Tool definition for function calling
/// </summary>
public record Tool(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("function")] FunctionDefinition Function
);
