using System.Text.Json.Serialization;

namespace MCP.Application.DTOs.AI.ChatCompletion;

public enum ChatFinishReason
{
    [JsonPropertyName("stop")]
    Stop = 1,
    [JsonPropertyName("length")]
    Length = 2,
    [JsonPropertyName("tool_calls")]
    ToolCalls = 3,
    [JsonPropertyName("content_filter")]
    ContentFilter = 4
}