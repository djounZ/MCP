using MCP.Application.DTOs.AI.ChatCompletion;

namespace MCP.Infrastructure.Services;

public interface IChatService
{
    Task<ChatResponseAppModel> GetResponseAsync(IEnumerable<ChatMessageAppModel> messagesAppModel, ChatOptionsAppModel? optionsAppModel = null,
        CancellationToken cancellationToken = new());

    IAsyncEnumerable<ChatResponseUpdateAppModel> GetStreamingResponseAsync(IEnumerable<ChatMessageAppModel> messagesAppModel, ChatOptionsAppModel? optionsAppModel = null,
        CancellationToken cancellationToken = new());
}