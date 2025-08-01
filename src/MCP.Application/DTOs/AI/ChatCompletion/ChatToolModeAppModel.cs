using System.Text.Json.Serialization;

namespace MCP.Application.DTOs.AI.ChatCompletion;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(NoneChatToolModeAppModel), typeDiscriminator: "none")]
[JsonDerivedType(typeof(AutoChatToolModeAppModel), typeDiscriminator: "auto")]
[JsonDerivedType(typeof(RequiredChatToolModeAppModel), typeDiscriminator: "required")]
public abstract record ChatToolModeAppModel;