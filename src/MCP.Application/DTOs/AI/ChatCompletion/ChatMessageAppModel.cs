using System.Text.Json.Serialization;
using MCP.Application.DTOs.AI.Contents;

namespace MCP.Application.DTOs.AI.ChatCompletion;

public sealed record ChatMessageAppModel(
    [property: JsonPropertyName("role")] ChatRoleEnumAppModel Role,
    [property: JsonPropertyName("contents")]  IList<AiContentAppModel> Contents
);
