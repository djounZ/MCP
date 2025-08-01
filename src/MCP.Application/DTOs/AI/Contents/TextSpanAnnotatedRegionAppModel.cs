using System.Text.Json.Serialization;

namespace MCP.Application.DTOs.AI.Contents;

public sealed record TextSpanAnnotatedRegionAppModel(
    [property: JsonPropertyName("start")] int StartIndex,
    [property: JsonPropertyName("end")] int EndIndex) : AnnotatedRegionAppModel;