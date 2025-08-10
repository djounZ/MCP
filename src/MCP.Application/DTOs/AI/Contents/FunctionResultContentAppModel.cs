using System.Text.Json.Serialization;

namespace MCP.Application.DTOs.AI.Contents;

public sealed record FunctionResultContentAppModel(
    IList<AiAnnotationAppModel>? Annotations,
    [property: JsonPropertyName("call_id")] string CallId,
    [property: JsonPropertyName("result")] object? Result)
    : AiContentAppModel(Annotations);