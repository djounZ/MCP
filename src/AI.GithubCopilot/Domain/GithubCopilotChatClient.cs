using AI.GithubCopilot.Infrastructure.Services;
using Microsoft.Extensions.AI;
using static AI.GithubCopilot.Infrastructure.Mappers.GithubCopilotChatCompletionMappers;

namespace AI.GithubCopilot.Domain;

public sealed class GithubCopilotChatClient(GithubCopilotChatCompletion githubCopilotChatCompletion) : IChatClient
{
    public void Dispose()
    {
       // no need to dispose anything;
    }

    public async Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null,
        CancellationToken cancellationToken = new())
    {
        var chatCompletionRequest = messages.ToChatCompletionRequest(options);
        var chatCompletionResponse = await githubCopilotChatCompletion.GetChatCompletionAsync(chatCompletionRequest, cancellationToken);
        return chatCompletionResponse.ToChatResponse();
    }

    public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null,
        CancellationToken cancellationToken = new())
    {
        var chatCompletionRequest = messages.ToChatCompletionRequest(options);
        var chatCompletionStreamAsync = githubCopilotChatCompletion.GetChatCompletionStreamAsync(chatCompletionRequest, cancellationToken);
        return chatCompletionStreamAsync.ToChatResponseUpdateStream(cancellationToken);
    }

    public object? GetService(Type serviceType, object? serviceKey = null)
    {
        return null;
    }
}
