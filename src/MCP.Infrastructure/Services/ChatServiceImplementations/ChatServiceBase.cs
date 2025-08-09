using System.Runtime.CompilerServices;
using MCP.Application.DTOs.AI;
using MCP.Application.DTOs.AI.ChatCompletion;
using MCP.Infrastructure.Models.Mappers;
using Microsoft.Extensions.AI;

namespace MCP.Infrastructure.Services.ChatServiceImplementations;

public abstract class ChatServiceBase<TChatClient>(TChatClient chatClient,ChatClientExtensionsAiAppModelsMapper mapper, AiToolsManager aiToolsManager) : IChatService where TChatClient : IChatClient
{


    public async Task<ChatResponseAppModel> GetResponseAsync(IEnumerable<ChatMessageAppModel> messagesAppModel, ChatOptionsAppModel? optionsAppModel = null,
        CancellationToken cancellationToken = new())
    {
        var (messages,options) = await  MapAsync(messagesAppModel, optionsAppModel, cancellationToken);
        var chatCompletionResponse = await chatClient.GetResponseAsync(messages, options, cancellationToken);
        return mapper.MapToAppModel(chatCompletionResponse);
    }

    public async IAsyncEnumerable<ChatResponseUpdateAppModel> GetStreamingResponseAsync(IEnumerable<ChatMessageAppModel> messagesAppModel, ChatOptionsAppModel? optionsAppModel = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = new())
    {
        var (messages,options) = await MapAsync(messagesAppModel, optionsAppModel, cancellationToken);
        var chatCompletionStreamAsync = chatClient.GetStreamingResponseAsync(messages, options, cancellationToken);

        await foreach (var update in chatCompletionStreamAsync)
        {
            if (update.Contents.Count >0 && update.Contents.All(c => c is FunctionCallContent || c is FunctionResultContent))
            {
                continue;
            }
            yield return mapper.MapToAppModel(update);
        }
    }

    private async Task<(IEnumerable<ChatMessage> Messages,ChatOptions? Options)> MapAsync(IEnumerable<ChatMessageAppModel> messagesAppModel, ChatOptionsAppModel? optionsAppModel, CancellationToken cancellationToken)
    {
        var messages = messagesAppModel.Select(mapper.MapFromAppModel);
        var options = mapper.MapFromAppModel(optionsAppModel);
        if (options != null)
        {
            options.Tools= await MapFromAppModelAsync(optionsAppModel?.Tools, cancellationToken);
        }
        return (messages, options);
    }
    private async Task<IList<AITool>?> MapFromAppModelAsync(IDictionary<string,IList<AiToolAppModel>>? chatOptionsTools, CancellationToken cancellationToken)
    {
        if (chatOptionsTools is null)
        {
            return null;
        }
        var aiTools = new List<AITool>();

        foreach (var chatOptionsTool in chatOptionsTools)
        {
            aiTools.AddRange(await MapFromAppModelAsync(chatOptionsTool.Key, chatOptionsTool.Value, cancellationToken));
        }
        return aiTools;
    }


    private async Task<IList<AITool>> MapFromAppModelAsync(string server,IList<AiToolAppModel> tools, CancellationToken cancellationToken)
    {
        return await aiToolsManager.GetAsync(server, [.. tools.Select(tool => tool.Name)], cancellationToken);
    }

}
