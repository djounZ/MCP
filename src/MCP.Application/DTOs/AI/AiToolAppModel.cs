using System.Text.Json.Serialization;

namespace MCP.Application.DTOs.AI;

public sealed record AiToolAppModel(
    [property: JsonPropertyName("name")] string Name);
