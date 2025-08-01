using System.Text.Json.Serialization;

namespace MCP.Application.DTOs.AI.Contents;

public sealed record TextContentAppModel(
    IList<AiAnnotationAppModel>? Annotations,
    [property: JsonPropertyName("text")] string? Text)
    : AiContentAppModel(Annotations);