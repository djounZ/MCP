using System.Collections.ObjectModel;
using System.Text.Json;
using MCP.Application.DTOs.AI.ChatCompletion;
using MCP.Application.DTOs.AI.Provider;
using MCP.Infrastructure.Services.ChatServiceImplementations;
using Microsoft.Extensions.DependencyInjection;

namespace MCP.Infrastructure.Services;

public sealed class ChatServiceManager
{

    private readonly IDictionary<AiProviderEnum, Type> _chatServicesByProvider = new ReadOnlyDictionary<AiProviderEnum, Type>(new Dictionary<AiProviderEnum, Type>
    {
        { AiProviderEnum.GithubCopilot, typeof(GithubCopilotChatService) },
        { AiProviderEnum.Ollama, typeof(OllamaChatService) }
    });

    private readonly IServiceProvider _serviceProvider;

    private readonly ReadOnlyDictionary<string, AiProviderEnum> _availableChatProviders;
    public ChatServiceManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _availableChatProviders = new ReadOnlyDictionary<string, AiProviderEnum>(
            _chatServicesByProvider.
                Keys.ToDictionary(provider => JsonSerializer.Serialize(provider).Replace("\"", string.Empty)));
    }

    private IChatService GetChatService(string? providerString)
    {
        var provider = AiProviderEnum.GithubCopilot;
        if (providerString  != null && !_availableChatProviders.TryGetValue(providerString, out provider) )
        {
            throw new ArgumentException($"Chat service for provider {providerString} is not registered.");
        }

        if (!_chatServicesByProvider.TryGetValue(provider, out var chatService))
        {
            throw new ArgumentException($"Chat service for provider {providerString} is not registered.");
        }

        return (IChatService) _serviceProvider.GetRequiredService(chatService);
    }

    public async Task<ChatResponseAppModel> GetResponseAsync(string? provider, IEnumerable<ChatMessageAppModel> messagesAppModel, ChatOptionsAppModel? optionsAppModel = null,
        CancellationToken cancellationToken = new())
    {
        return await GetChatService(provider).GetResponseAsync(messagesAppModel, optionsAppModel, cancellationToken);
    }



    public IAsyncEnumerable<ChatResponseUpdateAppModel> GetStreamingResponseAsync(string? provider, IEnumerable<ChatMessageAppModel> messagesAppModel, ChatOptionsAppModel? optionsAppModel = null,
        CancellationToken cancellationToken = new())
    {
        return  GetChatService(provider).GetStreamingResponseAsync(messagesAppModel, optionsAppModel, cancellationToken);
    }

}
