using System.Text.Json.Serialization;

namespace MCP.Application.DTOs.AI.Contents;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(DataContentAppModel), typeDiscriminator: "data")]
[JsonDerivedType(typeof(ErrorContentAppModel), typeDiscriminator: "error")]
[JsonDerivedType(typeof(FunctionCallContentAppModel), typeDiscriminator: "function_call")]
[JsonDerivedType(typeof(FunctionResultContentAppModel), typeDiscriminator: "function_result")]
[JsonDerivedType(typeof(TextContentAppModel), typeDiscriminator: "text")]
[JsonDerivedType(typeof(TextReasoningContentAppModel), typeDiscriminator: "reasoning")]
[JsonDerivedType(typeof(UriContentAppModel), typeDiscriminator: "uri")]
[JsonDerivedType(typeof(UsageContentAppModel), typeDiscriminator: "usage")]
public abstract record AiContentAppModel([property: JsonPropertyName("annotations")]IList<AiAnnotationAppModel>? Annotations);
