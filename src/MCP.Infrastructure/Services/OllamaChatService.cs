using MCP.Infrastructure.Models.Mappers;
using OllamaSharp;

namespace MCP.Infrastructure.Services;

public class OllamaChatService(
    OllamaApiClient ollamaApiClient,
    ChatClientExtensionsAiAppModelsMapper mapper)
    : ChatServiceBase<OllamaApiClient>(ollamaApiClient, mapper);
