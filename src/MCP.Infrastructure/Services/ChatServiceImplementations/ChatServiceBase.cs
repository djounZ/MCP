using System.Runtime.CompilerServices;
using MCP.Application.DTOs.AI.ChatCompletion;
using MCP.Infrastructure.Models.Mappers;
using Microsoft.Extensions.AI;

namespace MCP.Infrastructure.Services.ChatServiceImplementations;

public abstract class ChatServiceBase<TChatClient>(TChatClient chatClient,ChatClientExtensionsAiAppModelsMapper mapper) : IChatService where TChatClient : IChatClient
{


    public async Task<ChatResponseAppModel> GetResponseAsync(IEnumerable<ChatMessageAppModel> messagesAppModel, ChatOptionsAppModel? optionsAppModel = null,
        CancellationToken cancellationToken = new())
    {
        var messages = messagesAppModel.Select(mapper.MapFromAppModel);
        var options = mapper.MapFromAppModel(optionsAppModel);
        var chatCompletionResponse = await chatClient.GetResponseAsync(messages, options, cancellationToken);
        return mapper.MapToAppModel(chatCompletionResponse);
    }



    public async IAsyncEnumerable<ChatResponseUpdateAppModel> GetStreamingResponseAsync(IEnumerable<ChatMessageAppModel> messagesAppModel, ChatOptionsAppModel? optionsAppModel = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = new())
    {
        var messages = messagesAppModel.Select(mapper.MapFromAppModel);
        var options = mapper.MapFromAppModel(optionsAppModel);
        var chatCompletionStreamAsync = chatClient.GetStreamingResponseAsync(messages, options, cancellationToken);

        await foreach (var update in chatCompletionStreamAsync)
        {
            yield return mapper.MapToAppModel(update);
        }
    }

}
