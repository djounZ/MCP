using System.Collections.Concurrent;
using MCP.Tools.Infrastructure.Mappers;
using MCP.Tools.Infrastructure.Models;
using ModelContextProtocol.Client;

namespace MCP.Tools.Infrastructure.Services;

public class McpClientToolProviderService(McpServerConfigurationProviderService mcpServerConfigurationProviderService, ClientTransportFactoryService clientTransportFactoryService, McpServerConfigurationMapper mcpServerConfigurationMapper)
{


    private readonly ConcurrentDictionary<string, ServerValue > _serversDescription = new();

    private record ServerValue(McpServerConfigurationItem ServerConfiguration, IList<McpToolDescription> McpToolDescriptions);
    public async Task<IDictionary<string, IList<McpToolDescription>>> GetAll(CancellationToken cancellationToken)
    {
        var mcpToolDescriptions = new Dictionary<string, IList<McpToolDescription>>();
        var servers =  mcpServerConfigurationProviderService.GetMcpServerConfiguration()!.Servers;

        foreach (var (serverName, serverConfiguration) in servers)
        {
            if(_serversDescription.TryGetValue(serverName, out var serverToolDescriptions) && serverToolDescriptions.ServerConfiguration.Equals(serverConfiguration))
            {
                mcpToolDescriptions[serverName] = serverToolDescriptions.McpToolDescriptions;
                continue;
            }
            var clientTransport = clientTransportFactoryService.Create(serverConfiguration);
            await using IMcpClient client = await McpClientFactory.CreateAsync(clientTransport, cancellationToken: cancellationToken);
            var mcpClientTools = await client.ListToolsAsync(cancellationToken: cancellationToken);
            var toolDescriptions = mcpServerConfigurationMapper.Map(mcpClientTools);
            mcpToolDescriptions[serverName] = toolDescriptions;
            _serversDescription[serverName] = new ServerValue(serverConfiguration, toolDescriptions);
        }
        return mcpToolDescriptions;
    }
}
