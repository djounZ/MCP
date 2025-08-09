using System.Collections.Concurrent;
using AI.GithubCopilot.Infrastructure.Services;
using MCP.Application.DTOs.AI.Provider;
using MCP.Tools.Infrastructure.Services;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using OllamaSharp;

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

public sealed class AiProviderManager(GithubCopilotChatCompletion githubCopilotChatCompletion, OllamaApiClient ollamaApiClient)
{


    public async Task<IList<AiProviderAppModel>> GetAvailableAiProvidersAsync(CancellationToken token)
    {
        var githubCopilotModels = await GetGithubCopilotModels(token);
        var ollamaModels = await GetOllamaModels(token);

        return [githubCopilotModels, ollamaModels];
    }

    private async Task<AiProviderAppModel> GetOllamaModels(CancellationToken token)
    {
        var modelsResponse = await ollamaApiClient.ListLocalModelsAsync(token);
        var ollamaModels = new AiProviderAppModel(
            AiProviderEnum.Ollama,
            [.. modelsResponse.Select(model => new AiProviderAiModelAppModel(model.Name, model.Name))]);
        return ollamaModels;
    }

    private async Task<AiProviderAppModel> GetGithubCopilotModels(CancellationToken token)
    {
        var modelsResponse = await githubCopilotChatCompletion.GetModelsAsync(token);
        var githubCopilotModels = new AiProviderAppModel(
            AiProviderEnum.GithubCopilot,
            [.. modelsResponse.Data.Select(model => new AiProviderAiModelAppModel(model.Id, model.Name))]);
        return githubCopilotModels;
    }
}
