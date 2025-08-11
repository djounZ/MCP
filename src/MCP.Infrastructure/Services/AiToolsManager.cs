using System.Collections.Concurrent;
using MCP.Tools.Infrastructure.Services;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;

namespace MCP.Infrastructure.Services;

public sealed class AiToolsManager(McpServerConfigurationProviderService mcpServerConfigurationProviderService, ClientTransportFactoryService clientTransportFactoryService):IAsyncDisposable
{
    private readonly ConcurrentDictionary<string, Server> _clients = new();

    private sealed record Server(IMcpClient Client, IList<AITool> Tools) : IAsyncDisposable
    {
        public async ValueTask DisposeAsync()
        {
            await Client.DisposeAsync();
        }

    }
    public async Task<IList<AITool>> GetAsync(string serverName, List<string> requiredTools, CancellationToken cancellationToken)
    {

        if (_clients.TryGetValue(serverName, out var serverEntity))
        {
            return [.. serverEntity.Tools.Where(t=> requiredTools.Contains(t.Name))];
        }

        serverEntity = await GetAsync(serverName,cancellationToken);
        ValueTask? disposeTask = null;
        _clients.AddOrUpdate(serverName,  _ => serverEntity,(_,old)=>
        {
            disposeTask =old.DisposeAsync();
            return serverEntity;
        });
        if (disposeTask != null)
        {
            await disposeTask.Value;
        }
        return [.. serverEntity.Tools.Where(tool => requiredTools.Contains(tool.Name))];


    }

    private async Task<Server> GetAsync(string serverName, CancellationToken cancellationToken)
    {
        var serverConfiguration = mcpServerConfigurationProviderService.GetServer(serverName)!;
        var clientTransport = clientTransportFactoryService.Create(serverConfiguration);
        var client = await McpClientFactory.CreateAsync(clientTransport, cancellationToken: cancellationToken);
        var mcpClientTools = await client.ListToolsAsync(cancellationToken: cancellationToken);
        return new Server(client, [.. mcpClientTools]);
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var keyValuePair in _clients)
        {
            await keyValuePair.Value.DisposeAsync();
        }
    }
}