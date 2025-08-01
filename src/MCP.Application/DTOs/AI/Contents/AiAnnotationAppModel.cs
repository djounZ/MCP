using System.Text.Json.Serialization;

namespace MCP.Application.DTOs.AI.Contents;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(CitationAnnotationAppModel), typeDiscriminator: "citation")]
public abstract record AiAnnotationAppModel(
    [property: JsonPropertyName("annotated_regions")]  IList<AnnotatedRegionAppModel>? AnnotatedRegions);
