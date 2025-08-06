using AI.GithubCopilot.Infrastructure.Services;
using MCP.Application.DTOs.AI.Provider;
using OllamaSharp;

namespace MCP.Infrastructure.Services;

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
