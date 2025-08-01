using System.Text.Json.Serialization;

namespace MCP.Application.DTOs.AI.Contents;

public sealed record DataContentAppModel(
    IList<AiAnnotationAppModel>? Annotations,
    [property: JsonPropertyName("uri")] Uri Uri,
    [property: JsonPropertyName("media_type")] string? MediaType,
    [property: JsonPropertyName("name")] string? Name) : AiContentAppModel(Annotations);