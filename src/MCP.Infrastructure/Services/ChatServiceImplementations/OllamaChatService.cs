using MCP.Infrastructure.Models.Mappers;
using OllamaSharp;

namespace MCP.Infrastructure.Services.ChatServiceImplementations;

public class OllamaChatService(
    OllamaApiClient ollamaApiClient,
    ChatClientExtensionsAiAppModelsMapper mapper,
    AiToolsManager aiToolsManager)
    : ChatServiceBase<OllamaApiClient>(ollamaApiClient, mapper, aiToolsManager);
