using System.Text.Json.Serialization;

namespace MCP.Application.DTOs.AI.Contents;

public sealed record TextReasoningContentAppModel(
    IList<AiAnnotationAppModel>? Annotations,
    [property: JsonPropertyName("text")] string? Text)
    : AiContentAppModel(Annotations);