using System.Text.Json;
using System.Text.Json.Nodes;
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

public sealed record McpClientToolRequest(
    [property: JsonPropertyName("server_name")]string ServerName,
    [property: JsonPropertyName("tool_name")]string ToolName,
    [property: JsonPropertyName("arguments")]IReadOnlyDictionary<string, object?>? Arguments
);


[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(McpClientToolTextContentBlock), typeDiscriminator: "text")]
[JsonDerivedType(typeof(McpClientToolImageContentBlock), typeDiscriminator: "image")]
[JsonDerivedType(typeof(McpClientToolAudioContentBlock), typeDiscriminator: "audio")]
[JsonDerivedType(typeof(McpClientToolEmbeddedResourceBlock), typeDiscriminator: "resource")]
[JsonDerivedType(typeof(McpClientToolResourceLinkBlock), typeDiscriminator: "resource_link")]
public abstract record McpClientToolContentBlock(
    [property: JsonPropertyName("annotations")]McpClientAnnotationsDescription? Annotations
);

public sealed record McpClientToolTextContentBlock(
    McpClientAnnotationsDescription? Annotations,
    [property: JsonPropertyName("text")]string Text,
    [property: JsonPropertyName("_meta")]JsonObject? Meta
    ) : McpClientToolContentBlock(Annotations);

public sealed record McpClientToolImageContentBlock(
    McpClientAnnotationsDescription? Annotations,
    [property: JsonPropertyName("data")]string Data,
    [property: JsonPropertyName("media_type")]string MediaType,
    [property: JsonPropertyName("_meta")]JsonObject? Meta
) : McpClientToolContentBlock(Annotations);


public sealed record McpClientToolAudioContentBlock(
    McpClientAnnotationsDescription? Annotations,
    [property: JsonPropertyName("data")]string Data,
    [property: JsonPropertyName("media_type")]string MediaType,
    [property: JsonPropertyName("_meta")]JsonObject? Meta
) : McpClientToolContentBlock(Annotations);
public sealed record McpClientToolEmbeddedResourceBlock(
    McpClientAnnotationsDescription? Annotations,
    [property: JsonPropertyName("resource")] McpClientToolResourceContents Resource,
    [property: JsonPropertyName("_meta")]JsonObject? Meta
) : McpClientToolContentBlock(Annotations);

public sealed record McpClientToolResourceLinkBlock(
    McpClientAnnotationsDescription? Annotations,
    [property: JsonPropertyName("uri")]string Uri,
    [property: JsonPropertyName("name")]string Name,
    [property: JsonPropertyName("description")]string? Description,
    [property: JsonPropertyName("media_type")]string? MediaType,
    [property: JsonPropertyName("size")]long? Size
) : McpClientToolContentBlock(Annotations);



[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(McpClientToolBlobResourceContents), typeDiscriminator: "blob")]
[JsonDerivedType(typeof(McpClientToolTextResourceContents), typeDiscriminator: "text")]
public abstract record McpClientToolResourceContents(
    [property: JsonPropertyName("uri")]string Uri,
    [property: JsonPropertyName("media_type")]string? MediaType,
    [property: JsonPropertyName("_meta")]JsonObject? Meta
);

public sealed record McpClientToolBlobResourceContents(
    string Uri,
    string? MediaType,
    JsonObject? Meta,
    [property: JsonPropertyName("blob")]string Blob
):McpClientToolResourceContents(Uri, MediaType, Meta);

public sealed record McpClientToolTextResourceContents(
    string Uri,
    string? MediaType,
    JsonObject? Meta,
    [property: JsonPropertyName("text")]string Text
):McpClientToolResourceContents(Uri, MediaType, Meta);
public sealed record McpClientToolResponse(
    [property: JsonPropertyName("content")]IList<McpClientToolContentBlock> Content,
    [property: JsonPropertyName("structured_content")]JsonNode? StructuredContent,
    [property: JsonPropertyName("isError")]bool? IsError
);
