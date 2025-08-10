using MCP.Infrastructure.Models.Mappers;
using Microsoft.Extensions.Logging;
using OllamaSharp;

namespace MCP.Infrastructure.Services.ChatServiceImplementations;

public class OllamaChatService(
    ILoggerFactory loggerFactory,
    OllamaApiClient ollamaApiClient,
    ChatClientExtensionsAiAppModelsMapper mapper,
    AiToolsManager aiToolsManager)
    : ChatServiceBase<OllamaApiClient>(loggerFactory, ollamaApiClient, mapper, aiToolsManager);
