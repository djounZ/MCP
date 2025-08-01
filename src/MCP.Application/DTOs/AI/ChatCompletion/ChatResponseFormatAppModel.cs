using System.Text.Json.Serialization;

namespace MCP.Application.DTOs.AI.ChatCompletion;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(ChatResponseFormatTextAppModel), typeDiscriminator: "text")]
[JsonDerivedType(typeof(ChatResponseFormatJsonAppModel), typeDiscriminator: "json")]
public abstract record ChatResponseFormatAppModel;