using System.Text.Json.Serialization;

namespace MCP.Application.DTOs.AI.ChatCompletion;

public enum ChatRoleEnumAppModel
{
    [JsonPropertyName("system")]
    System = 1,
    [JsonPropertyName("assistant")]
    Assistant = 2,
    [JsonPropertyName("user")]
    User = 3,
    [JsonPropertyName("tool")]
    Tool = 4
}