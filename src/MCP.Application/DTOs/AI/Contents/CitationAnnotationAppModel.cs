using System.Text.Json.Serialization;

namespace MCP.Application.DTOs.AI.Contents;

public sealed record CitationAnnotationAppModel(
    IList<AnnotatedRegionAppModel>? AnnotatedRegions,
    [property: JsonPropertyName("title")] string? Title,
    [property: JsonPropertyName("url")] Uri Url,
    [property: JsonPropertyName("file_id")] string? FileId,
    [property: JsonPropertyName("tool_name")] string? ToolName,
    [property: JsonPropertyName("snippet")] string? Snippet) : AiAnnotationAppModel(AnnotatedRegions);