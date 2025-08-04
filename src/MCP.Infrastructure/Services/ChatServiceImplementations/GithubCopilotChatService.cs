using AI.GithubCopilot.Domain;
using MCP.Infrastructure.Models.Mappers;

namespace MCP.Infrastructure.Services.ChatServiceImplementations;

public class GithubCopilotChatService(
    GithubCopilotChatClient githubCopilotChatClient,
    ChatClientExtensionsAiAppModelsMapper mapper)
    : ChatServiceBase<GithubCopilotChatClient>(githubCopilotChatClient, mapper);
