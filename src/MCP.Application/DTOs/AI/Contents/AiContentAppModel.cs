using System.Text.Json.Serialization;

namespace MCP.Application.DTOs.AI.Contents;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(DataContentAppModel), typeDiscriminator: "data")]
[JsonDerivedType(typeof(ErrorContentAppModel), typeDiscriminator: "error")]
// [JsonDerivedType(typeof(FunctionCallContent), typeDiscriminator: "functionCall")]
// [JsonDerivedType(typeof(FunctionResultContent), typeDiscriminator: "functionResult")]
[JsonDerivedType(typeof(TextContentAppModel), typeDiscriminator: "text")]
[JsonDerivedType(typeof(TextReasoningContentAppModel), typeDiscriminator: "reasoning")]
[JsonDerivedType(typeof(UriContentAppModel), typeDiscriminator: "uri")]
[JsonDerivedType(typeof(UsageContentAppModel), typeDiscriminator: "usage")]
public abstract record AiContentAppModel([property: JsonPropertyName("annotations")]IList<AiAnnotationAppModel>? Annotations);
