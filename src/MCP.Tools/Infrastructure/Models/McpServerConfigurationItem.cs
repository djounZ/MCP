using System.Text.Json;
using System.Text.Json.Serialization;

namespace MCP.Tools.Infrastructure.Models;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(McpServerConfigurationItemStdio), typeDiscriminator: "stdio")]
[JsonDerivedType(typeof(McpServerConfigurationItemHttp), typeDiscriminator: "stdio")]
public abstract record McpServerConfigurationItem(
    [property: JsonPropertyName("category")] string? Category);
public sealed record McpServerConfigurationItemStdio(
    string? Category,
    [property: JsonPropertyName("command")] string Command,
    [property: JsonPropertyName("args")] IList<string>? Arguments,
    [property: JsonPropertyName("env")] IDictionary<string, string?>? EnvironmentVariables
    ):McpServerConfigurationItem(Category);
public sealed record McpServerConfigurationItemHttp(
    string? Category,
    [property: JsonPropertyName("url")] Uri Endpoint
):McpServerConfigurationItem(Category);


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
