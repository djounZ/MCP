using System.Text.Json;
using System.Text.Json.Serialization;

namespace MCP.Tools.Infrastructure.Models;



[JsonConverter(typeof(JsonStringEnumConverter))]
public enum McpServerTransportType
{
    [JsonStringEnumMemberName("stdio")]
    Stdio = 1,
    [JsonStringEnumMemberName("http")]
    Http = 2
}

public sealed record McpServerConfigurationItem(
    [property: JsonPropertyName("category")] string? Category,
    [property: JsonPropertyName("command")] string? Command,
    [property: JsonPropertyName("args")] IList<string>? Arguments,
    [property: JsonPropertyName("env")] IDictionary<string, string?>? EnvironmentVariables,
    [property: JsonPropertyName("url")] Uri? Endpoint,
    [property: JsonPropertyName("type")] McpServerTransportType Type = McpServerTransportType.Stdio);


public sealed record McpServerConfiguration(
    [property: JsonPropertyName("servers")]
    IDictionary<string, McpServerConfigurationItem> Servers);


public sealed record McpToolDescription(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("description")]
    string? Description,
    [property: JsonPropertyName("json_schema")]
    JsonElement JsonSchema,
    [property: JsonPropertyName("return_json_schema")]
    JsonElement? ReturnJsonSchema);
