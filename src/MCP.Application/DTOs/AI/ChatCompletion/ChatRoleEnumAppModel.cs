using System.Text.Json.Serialization;

namespace MCP.Application.DTOs.AI.ChatCompletion;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ChatRoleEnumAppModel
{
    [JsonStringEnumMemberName("system")]
    System = 1,
    [JsonStringEnumMemberName("assistant")]
    Assistant = 2,
    [JsonStringEnumMemberName("user")]
    User = 3,
    [JsonStringEnumMemberName("tool")]
    Tool = 4
}
