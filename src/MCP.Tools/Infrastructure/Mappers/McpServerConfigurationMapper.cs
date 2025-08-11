using MCP.Tools.Infrastructure.Models;
using ModelContextProtocol.Client;

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
                proto.Annotations.Audience?.Select(r => r == ModelContextProtocol.Protocol.Role.Assistant ? McpClientRoleDescription.Assistant : McpClientRoleDescription.Tool).ToList(),
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
                proto.Annotations.Audience?.Select(r => r == ModelContextProtocol.Protocol.Role.Assistant ? McpClientRoleDescription.Assistant : McpClientRoleDescription.Tool).ToList(),
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
}
