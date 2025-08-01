using System.Text.Json.Serialization;

namespace MCP.Application.DTOs.AI.Contents;

public sealed record UsageContentAppModel(
    IList<AiAnnotationAppModel>? Annotations,
    [property: JsonPropertyName("details")] UsageDetailsAppModel DetailsAppModel)
    : AiContentAppModel(Annotations);