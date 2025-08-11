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


public sealed record McpClientToolDescription(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("description")]
    string? Description,
    [property: JsonPropertyName("input_schema")]
    JsonElement InputSchema,
    [property: JsonPropertyName("output_schema")]
    JsonElement? OutputSchema);


public sealed record McpClientPromptArgumentDescription(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("title")] string? Title,
    [property: JsonPropertyName("description")]
    string? Description,
    [property: JsonPropertyName("required")]
    bool? Required);
public sealed record McpClientPromptDescription(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("title")] string? Title,
    [property: JsonPropertyName("description")]
    string? Description,
    [property: JsonPropertyName("arguments")]
    IList<McpClientPromptArgumentDescription>? Arguments,
    [property: JsonPropertyName("_meta")]
    JsonElement? Meta);


[JsonConverter(typeof(JsonStringEnumConverter))]
public enum McpClientRoleDescription
{
    [JsonStringEnumMemberName("assistant")]
    Assistant = 2,
    [JsonStringEnumMemberName("user")]
    Tool = 4
}

public sealed record McpClientAnnotationsDescription(
    [property: JsonPropertyName("audience")] IList<McpClientRoleDescription>? Audience,
    [property: JsonPropertyName("priority")] float? Priority,
    [property: JsonPropertyName("lastModified")] DateTimeOffset? LastModified);

public sealed record McpClientResourceTemplateDescription(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("title")] string? Title,
    [property: JsonPropertyName("uriTemplate")] string? UriTemplate,
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("mimeType")] string? MimeType,
    [property: JsonPropertyName("annotations")] McpClientAnnotationsDescription? Annotations,
    [property: JsonPropertyName("_meta")]
    JsonElement? Meta);



public sealed record McpClientResourceDescription(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("title")] string? Title,
    [property: JsonPropertyName("uri")] string? Uri,
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("mimeType")] string? MimeType,
    [property: JsonPropertyName("annotations")] McpClientAnnotationsDescription? Annotations,
    [property: JsonPropertyName("size")] long? Size,
    [property: JsonPropertyName("_meta")] JsonElement? Meta);


public sealed record McpClientDescription(
    [property: JsonPropertyName("tools")] IList<McpClientToolDescription> Tools,
    [property: JsonPropertyName("prompts")] IList<McpClientPromptDescription> Prompts,
    [property: JsonPropertyName("resource_templates")] IList<McpClientResourceTemplateDescription> ResourceTemplates,
    [property: JsonPropertyName("resources")] IList<McpClientResourceDescription> Resources
    );
