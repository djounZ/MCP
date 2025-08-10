using AI.GithubCopilot.Domain;
using MCP.Infrastructure.Models.Mappers;
using Microsoft.Extensions.Logging;

namespace MCP.Infrastructure.Services.ChatServiceImplementations;

public class GithubCopilotChatService(
    ILoggerFactory loggerFactory,
    GithubCopilotChatClient githubCopilotChatClient,
    ChatClientExtensionsAiAppModelsMapper mapper,
    AiToolsManager aiToolsManager)
    : ChatServiceBase<GithubCopilotChatClient>(loggerFactory, githubCopilotChatClient, mapper, aiToolsManager);
