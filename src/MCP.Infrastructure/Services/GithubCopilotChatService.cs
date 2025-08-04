using AI.GithubCopilot.Domain;
using MCP.Infrastructure.Models.Mappers;

namespace MCP.Infrastructure.Services;

public class GithubCopilotChatService(
    GithubCopilotChatClient githubCopilotChatClient,
    ChatClientExtensionsAiAppModelsMapper mapper)
    : ChatServiceBase<GithubCopilotChatClient>(githubCopilotChatClient, mapper);
