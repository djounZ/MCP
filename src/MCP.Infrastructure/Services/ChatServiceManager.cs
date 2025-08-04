using System.Collections.ObjectModel;
using System.Text.Json;
using MCP.Application.DTOs.AI.ChatCompletion;
using MCP.Infrastructure.Services.ChatServiceImplementations;
using Microsoft.Extensions.DependencyInjection;

namespace MCP.Infrastructure.Services;

public sealed class ChatServiceManager
{

    private readonly IDictionary<ChatClientProviderEnum, Type> _chatServicesByProvider = new ReadOnlyDictionary<ChatClientProviderEnum, Type>(new Dictionary<ChatClientProviderEnum, Type>
    {
        { ChatClientProviderEnum.GithubCopilot, typeof(GithubCopilotChatService) },
        { ChatClientProviderEnum.Ollama, typeof(OllamaChatService) }
    });

    private readonly IServiceProvider _serviceProvider;

    private readonly ReadOnlyDictionary<string, ChatClientProviderEnum> _availableChatProviders;
    public ChatServiceManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _availableChatProviders = new ReadOnlyDictionary<string, ChatClientProviderEnum>(
            _chatServicesByProvider.
                Keys.ToDictionary(provider => JsonSerializer.Serialize(provider).Replace("\"", string.Empty)));
    }

    private IChatService GetChatService(string? providerString)
    {
        var provider = ChatClientProviderEnum.GithubCopilot;
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

    public IEnumerable<string> GetAvailableChatProviders()
    {
        return _availableChatProviders.Keys;
    }
}
