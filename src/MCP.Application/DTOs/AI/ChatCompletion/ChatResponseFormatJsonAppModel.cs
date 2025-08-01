using System.Text.Json;
using System.Text.Json.Serialization;

namespace MCP.Application.DTOs.AI.ChatCompletion;

public sealed record ChatResponseFormatJsonAppModel(
    [property: JsonPropertyName("schema")] JsonElement? Schema,
    [property: JsonPropertyName("schema_name")] string? SchemaName,
    [property: JsonPropertyName("schema_description")] string? SchemaDescription
) : ChatResponseFormatAppModel;