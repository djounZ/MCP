using System.Runtime.CompilerServices;
using AI.GithubCopilot.Domain;
using MCP.Application.DTOs.AI.ChatCompletion;
using MCP.Infrastructure.Models.Mappers;

namespace MCP.Infrastructure.Services;

public class GithubCopilotChatService(GithubCopilotChatClient githubCopilotChatClient,GithubCopilotChatClientAppModelsMapper mapper)
{


    public async Task<ChatResponseAppModel> GetResponseAsync(IEnumerable<ChatMessageAppModel> messagesAppModel, ChatOptionsAppModel? optionsAppModel = null,
        CancellationToken cancellationToken = new())
    {
        var messages = messagesAppModel.Select(mapper.MapFromAppModel);
        var options = mapper.MapFromAppModel(optionsAppModel);
        var chatCompletionResponse = await githubCopilotChatClient.GetResponseAsync(messages, options, cancellationToken);
        return mapper.MapToAppModel(chatCompletionResponse);
    }



    public async IAsyncEnumerable<ChatResponseUpdateAppModel> GetStreamingResponseAsync(IEnumerable<ChatMessageAppModel> messagesAppModel, ChatOptionsAppModel? optionsAppModel = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = new())
    {
            var messages = messagesAppModel.Select(mapper.MapFromAppModel);
            var options = mapper.MapFromAppModel(optionsAppModel);
        var chatCompletionStreamAsync = githubCopilotChatClient.GetStreamingResponseAsync(messages, options, cancellationToken);

        await foreach (var update in chatCompletionStreamAsync)
        {
            yield return mapper.MapToAppModel(update);
        }
    }
}
