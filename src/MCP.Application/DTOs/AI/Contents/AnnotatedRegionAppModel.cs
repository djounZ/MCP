using System.Text.Json.Serialization;

namespace MCP.Application.DTOs.AI.Contents;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(TextSpanAnnotatedRegionAppModel), typeDiscriminator: "textSpan")]
public abstract record AnnotatedRegionAppModel();
