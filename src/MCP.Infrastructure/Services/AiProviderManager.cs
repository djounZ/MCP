using AI.GithubCopilot.Infrastructure.Services;
using MCP.Application.DTOs.AI.Provider;
using Microsoft.Extensions.Logging;
using OllamaSharp;

namespace MCP.Infrastructure.Services;

public sealed class AiProviderManager(ILogger<AiProviderManager> logger, GithubCopilotChatCompletion githubCopilotChatCompletion, OllamaApiClient ollamaApiClient)
{


    public async Task<IList<AiProviderAppModel>> GetAvailableAiProvidersAsync(CancellationToken token)
    {
        var aiProviderAppModels = new List<AiProviderAppModel>();

        var githubCopilotModels = await GetProviderModels(
            AiProviderEnum.GithubCopilot,
            githubCopilotChatCompletion.GetModelsAsync,
            modelsResponse=> modelsResponse.Data.Select(model => new AiProviderAiModelAppModel(model.Id, model.Name))
            ,token);

        if (githubCopilotModels != null)
        {
            aiProviderAppModels.Add(githubCopilotModels);
        }

        var ollamaModels = await GetProviderModels(
            AiProviderEnum.Ollama,
            ollamaApiClient.ListLocalModelsAsync,
            modelsResponse=> modelsResponse.Select(model => new AiProviderAiModelAppModel(model.Name, model.Name))
            ,token);

        if (ollamaModels != null)
        {
            aiProviderAppModels.Add(ollamaModels);
        }

        return aiProviderAppModels;
    }

    private async Task<AiProviderAppModel?> GetProviderModels<TProviderModels>(
        AiProviderEnum provider,
        Func<CancellationToken, Task<TProviderModels>> getFunc,
        Func<TProviderModels, IEnumerable<AiProviderAiModelAppModel>> transformFunc,
        CancellationToken token)
    {
        try
        {

            var modelsResponse = await getFunc(token);
            var aiProviderAppModel = new AiProviderAppModel(
                provider,
                [.. transformFunc(modelsResponse)]);
            return aiProviderAppModel;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to get {@Provider} models", provider);
            return null;
        }
    }
}
