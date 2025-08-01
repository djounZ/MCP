using System.Text.Json.Serialization;

namespace MCP.Application.DTOs.AI.Contents;

public sealed record UriContentAppModel(
    IList<AiAnnotationAppModel>? Annotations,
    [property: JsonPropertyName("uri")] Uri Uri,
    [property: JsonPropertyName("media_type")] string MediaType) : AiContentAppModel(Annotations);