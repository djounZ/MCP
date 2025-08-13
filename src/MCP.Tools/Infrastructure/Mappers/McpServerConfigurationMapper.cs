using MCP.Tools.Infrastructure.Models;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace MCP.Tools.Infrastructure.Mappers;

public class McpServerConfigurationMapper
{
    public StdioClientTransportOptions MapStdioClientTransportOptions(McpServerConfigurationItem configurationItem)
    {
        return new StdioClientTransportOptions
        {
            Command = configurationItem.Command!,
            Arguments = configurationItem.Arguments,
            EnvironmentVariables = configurationItem.EnvironmentVariables
        };
    }

    public SseClientTransportOptions MapSseClientTransport(McpServerConfigurationItem configurationItem)
    {
        return new SseClientTransportOptions
        {
            Endpoint = configurationItem.Endpoint!
        };
    }



    public IList<McpClientToolDescription> Map(IList<McpClientTool> clients)
    {
        return [.. clients.Select(Map)];
    }

    public McpClientToolDescription Map(McpClientTool tools)
    {
        return new McpClientToolDescription(
            tools.Name,
            tools.Description,
            tools.JsonSchema,
            tools.ReturnJsonSchema
        );
    }



    public IList<McpClientPromptDescription> Map(IList<McpClientPrompt> prompts)
    {
        return [.. prompts.Select(Map)];
    }

    public McpClientPromptDescription Map(McpClientPrompt prompt)
    {
        var proto = prompt.ProtocolPrompt;
        IList<McpClientPromptArgumentDescription>? args = null;
        if (proto.Arguments != null)
        {
            args = proto.Arguments.Select(a => new McpClientPromptArgumentDescription(
                a.Name,
                a.Title,
                a.Description,
                a.Required
            )).ToList();
        }
        return new McpClientPromptDescription(
            proto.Name,
            proto.Title,
            proto.Description,
            args,
            proto.Meta != null ? System.Text.Json.JsonSerializer.SerializeToElement(proto.Meta) : null
        );
    }



    public IList<McpClientResourceTemplateDescription> Map(IList<McpClientResourceTemplate> resourceTemplates)
    {
        return [.. resourceTemplates.Select(Map)];
    }

    public McpClientResourceTemplateDescription Map(McpClientResourceTemplate resourceTemplate)
    {
        var proto = resourceTemplate.ProtocolResourceTemplate;
        McpClientAnnotationsDescription? annotations = null;
        if (proto.Annotations != null)
        {
            annotations = new McpClientAnnotationsDescription(
                proto.Annotations.Audience?.Select(r => r == Role.Assistant ? McpClientRoleDescription.Assistant : McpClientRoleDescription.Tool).ToList(),
                proto.Annotations.Priority,
                proto.Annotations.LastModified
            );
        }
        return new McpClientResourceTemplateDescription(
            proto.Name,
            proto.Title,
            proto.UriTemplate,
            proto.Description,
            proto.MimeType,
            annotations,
            proto.Meta != null ? System.Text.Json.JsonSerializer.SerializeToElement(proto.Meta) : null
        );
    }


    public IList<McpClientResourceDescription> Map(IList<McpClientResource> resources)
    {
        return [.. resources.Select(Map)];
    }

    public McpClientResourceDescription Map(McpClientResource resource)
    {
        var proto = resource.ProtocolResource;
        McpClientAnnotationsDescription? annotations = null;
        if (proto.Annotations != null)
        {
            annotations = new McpClientAnnotationsDescription(
                proto.Annotations.Audience?.Select(r => r == Role.Assistant ? McpClientRoleDescription.Assistant : McpClientRoleDescription.Tool).ToList(),
                proto.Annotations.Priority,
                proto.Annotations.LastModified
            );
        }
        return new McpClientResourceDescription(
            proto.Name,
            proto.Title,
            proto.Uri,
            proto.Description,
            proto.MimeType,
            annotations,
            proto.Size,
            proto.Meta != null ? System.Text.Json.JsonSerializer.SerializeToElement(proto.Meta) : null
        );
    }

    public McpClientToolResponse Map(CallToolResult callToolResult)
    {
        IList<McpClientToolContentBlock> contentBlocks = [];
        foreach (var block in callToolResult.Content)
        {
            var contentBlock = Map(block);
            if (contentBlock != null)
            {
                contentBlocks.Add(contentBlock);
            }
        }

        return new McpClientToolResponse(
            contentBlocks,
            callToolResult.StructuredContent,
            callToolResult.IsError
        );
    }

    private static McpClientToolContentBlock? Map(ContentBlock block)
    {
        McpClientToolContentBlock? contentBlock = block switch
        {
            TextContentBlock text => new McpClientToolTextContentBlock(MapAnnotations(text.Annotations), text.Text,
                text.Meta),
            ImageContentBlock image => new McpClientToolImageContentBlock(MapAnnotations(image.Annotations),
                image.Data, image.MimeType, image.Meta),
            AudioContentBlock audio => new McpClientToolAudioContentBlock(MapAnnotations(audio.Annotations),
                audio.Data, audio.MimeType, audio.Meta),
            EmbeddedResourceBlock resource => new McpClientToolEmbeddedResourceBlock(
                MapAnnotations(resource.Annotations), MapResourceContents(resource.Resource), resource.Meta),
            ResourceLinkBlock link => new McpClientToolResourceLinkBlock(MapAnnotations(link.Annotations), link.Uri,
                link.Name, link.Description, link.MimeType, link.Size),
            _ => null
        };
        return contentBlock;
    }

    private static McpClientAnnotationsDescription? MapAnnotations(Annotations? annotations)
    {
        if (annotations == null)
        {
            return null;
        }

        return new McpClientAnnotationsDescription(
            annotations.Audience?.Select(r => r == Role.Assistant ? McpClientRoleDescription.Assistant : McpClientRoleDescription.Tool).ToList(),
            annotations.Priority,
            annotations.LastModified
        );
    }

    private static McpClientToolResourceContents MapResourceContents(ResourceContents resource)
    {
        return resource switch
        {
            BlobResourceContents blob => new McpClientToolBlobResourceContents(blob.Uri, blob.MimeType,
                blob.Meta, blob.Blob),
            TextResourceContents text => new McpClientToolTextResourceContents(text.Uri, text.MimeType,
                text.Meta, text.Text),
            _ => throw new NotSupportedException($"Unknown resource content type: {resource.GetType().Name}")
        };
    }
}
