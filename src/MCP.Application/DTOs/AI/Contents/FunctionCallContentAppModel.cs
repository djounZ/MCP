using System.Text.Json.Serialization;

namespace MCP.Application.DTOs.AI.Contents;

public sealed record FunctionCallContentAppModel(
    IList<AiAnnotationAppModel>? Annotations,
    [property: JsonPropertyName("call_id")] string CallId,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("arguments")] IDictionary<string, object?>? Arguments)
    : AiContentAppModel(Annotations);