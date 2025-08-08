using MCP.Tools.Infrastructure.Models;
using ModelContextProtocol.Client;

namespace MCP.Tools.Infrastructure.Mappers;

public class McpServerConfigurationMapper
{
    public StdioClientTransportOptions Map(McpServerConfigurationItemStdio configurationItem)
    {
        return new StdioClientTransportOptions
        {
            Command = configurationItem.Command,
            Arguments = configurationItem.Arguments,
            EnvironmentVariables = configurationItem.EnvironmentVariables
        };
    }

    public SseClientTransportOptions Map(McpServerConfigurationItemHttp configurationItem)
    {
        return new SseClientTransportOptions
        {
            Endpoint = configurationItem.Endpoint
        };
    }


    public IList<McpToolDescription> Map(IList<McpClientTool> clients)
    {
        return [.. clients.Select(Map)];
    }

    public McpToolDescription Map(McpClientTool client)
    {
        return new McpToolDescription(
            client.Name,
            client.Description,
            client.JsonSchema,
            client.ReturnJsonSchema
        );
    }
}
