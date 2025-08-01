using System.Text.Json.Serialization;

namespace MCP.Application.DTOs.AI.Contents;

public sealed record ErrorContentAppModel(
    IList<AiAnnotationAppModel>? Annotations,
    [property: JsonPropertyName("message")] string? Message)
    : AiContentAppModel(Annotations);