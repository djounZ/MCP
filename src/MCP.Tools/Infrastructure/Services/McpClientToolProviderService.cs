using System.Collections.Concurrent;
using MCP.Tools.Infrastructure.Mappers;
using MCP.Tools.Infrastructure.Models;
using Microsoft.Extensions.Logging;
using ModelContextProtocol;
using ModelContextProtocol.Client;

namespace MCP.Tools.Infrastructure.Services;

public class McpClientToolProviderService(ILogger<McpClientToolProviderService> logger, McpServerConfigurationProviderService mcpServerConfigurationProviderService, ClientTransportFactoryService clientTransportFactoryService, McpServerConfigurationMapper mcpServerConfigurationMapper)
{


    private readonly ConcurrentDictionary<string, ServerValue > _serversDescription = new();

    private record ServerValue(McpServerConfigurationItem ServerConfiguration, IList<McpClientToolDescription> McpToolDescriptions);
    public async Task<IDictionary<string, IList<McpClientToolDescription>>> GetAll(CancellationToken cancellationToken)
    {
        var mcpToolDescriptions = new Dictionary<string, IList<McpClientToolDescription>>();
        var servers =  mcpServerConfigurationProviderService.GetMcpServerConfiguration().Servers;

        foreach (var (serverName, serverConfiguration) in servers)
        {
            if(_serversDescription.TryGetValue(serverName, out var serverToolDescriptions) && serverToolDescriptions.ServerConfiguration.Equals(serverConfiguration))
            {
                mcpToolDescriptions[serverName] = serverToolDescriptions.McpToolDescriptions;
                continue;
            }
            var clientTransport = clientTransportFactoryService.Create(serverConfiguration);
            await using var client = await McpClientFactory.CreateAsync(clientTransport, cancellationToken: cancellationToken);
            var mcpClientTools = await client.ListToolsAsync(cancellationToken: cancellationToken);

            var toolDescriptions = mcpServerConfigurationMapper.Map(mcpClientTools);
            mcpToolDescriptions[serverName] = toolDescriptions;
            _serversDescription[serverName] = new ServerValue(serverConfiguration, toolDescriptions);
        }
        return mcpToolDescriptions;
    }


    public async Task<McpClientToolResponse> CallToolAsync(McpClientToolRequest request,
        CancellationToken cancellationToken)
    {
        var mcpServerConfigurationItem = mcpServerConfigurationProviderService.GetMcpServerConfiguration().Servers[request.ServerName];


        var clientTransport = clientTransportFactoryService.Create(mcpServerConfigurationItem);
        await using var client = await McpClientFactory.CreateAsync(clientTransport, cancellationToken: cancellationToken);

        var callToolResult = await client.CallToolAsync(request.ToolName, request.Arguments, _progress, null, cancellationToken);
        return mcpServerConfigurationMapper.Map(callToolResult);
    }

    private readonly McpClientToolCallProgressIProgress _progress = new(logger);
    private sealed class McpClientToolCallProgressIProgress(ILogger logger) :  IProgress<ProgressNotificationValue>{
        public void Report(ProgressNotificationValue value)
        {
           logger.LogInformation("Progression Report {@ProgressNotificationValue}",value);
        }
    }
}
