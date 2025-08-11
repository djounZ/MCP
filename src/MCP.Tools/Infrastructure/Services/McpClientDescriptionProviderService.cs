using System.Collections.Concurrent;
using MCP.Tools.Infrastructure.Mappers;
using MCP.Tools.Infrastructure.Models;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;

namespace MCP.Tools.Infrastructure.Services;

public class McpClientDescriptionProviderService(ILogger<McpClientDescriptionProviderService> logger, McpServerConfigurationProviderService mcpServerConfigurationProviderService, ClientTransportFactoryService clientTransportFactoryService, McpServerConfigurationMapper mcpServerConfigurationMapper)
{


    private readonly ConcurrentDictionary<string, ServerValue > _serversDescription = new();

    private record ServerValue(McpServerConfigurationItem ServerConfiguration, McpClientDescription McpToolDescriptions);
    public async Task<IDictionary<string, McpClientDescription>> GetAll(CancellationToken cancellationToken)
    {
        var mcpToolDescriptions = new Dictionary<string, McpClientDescription>();
        var servers =  mcpServerConfigurationProviderService.GetMcpServerConfiguration()!.Servers;

        foreach (var (serverName, serverConfiguration) in servers)
        {
            if(_serversDescription.TryGetValue(serverName, out var serverToolDescriptions) && serverToolDescriptions.ServerConfiguration.Equals(serverConfiguration))
            {
                mcpToolDescriptions[serverName] = serverToolDescriptions.McpToolDescriptions;
                continue;
            }
            var clientTransport = clientTransportFactoryService.Create(serverConfiguration);
            var client = await  McpClientFactory.CreateAsync(clientTransport, cancellationToken: cancellationToken);
            try
            {
                var mcpClientTools = RunSafeAsync(()=>client.ListToolsAsync(cancellationToken: cancellationToken));
                var mcpClientPrompts =  RunSafeAsync(()=>client.ListPromptsAsync(cancellationToken: cancellationToken));
                var mcpClientResourceTemplates =  RunSafeAsync(()=>client.ListResourceTemplatesAsync(cancellationToken: cancellationToken));
                var mcpClientResources =  RunSafeAsync(()=>client.ListResourcesAsync(cancellationToken: cancellationToken));
                var toolDescriptions = mcpServerConfigurationMapper.Map(await mcpClientTools);
                var promptDescriptions = mcpServerConfigurationMapper.Map(await mcpClientPrompts);
                var resourceTemplatesDescriptions = mcpServerConfigurationMapper.Map(await mcpClientResourceTemplates);
                var resourcesDescriptions = mcpServerConfigurationMapper.Map(await mcpClientResources);
                McpClientDescription mcpClientDescription = new(
                    toolDescriptions,
                    promptDescriptions,
                    resourceTemplatesDescriptions,
                    resourcesDescriptions


                );
                mcpToolDescriptions[serverName] = mcpClientDescription;
                _serversDescription[serverName] = new ServerValue(serverConfiguration, mcpClientDescription);
            }
            finally
            {
                await client.DisposeAsync();
            }
        }
        return mcpToolDescriptions;
    }

    private async ValueTask<IList<T>> RunSafeAsync<T>(Func<ValueTask<IList<T>>> fun)
    {
        try
        {
            return await fun();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while fetching MCP client descriptions");
            return [];
        }
    }
}
