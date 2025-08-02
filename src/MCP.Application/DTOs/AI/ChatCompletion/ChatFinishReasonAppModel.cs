using System.Text.Json.Serialization;

namespace MCP.Application.DTOs.AI.ChatCompletion;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ChatFinishReasonAppModel
{
    [JsonStringEnumMemberName("stop")]
    Stop = 1,
    [JsonStringEnumMemberName("length")]
    Length = 2,
    [JsonStringEnumMemberName("tool_calls")]
    ToolCalls = 3,
    [JsonStringEnumMemberName("content_filter")]
    ContentFilter = 4
}
