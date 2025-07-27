namespace AI.GithubCopilot.Infrastructure.Services;

public sealed class GithubCopilotChatAuthorizationHandler(GithubCopilotTokenProvider githubCopilotTokenProvider) : AbstractAuthorizationHandler
{
    protected override async Task<string> GetParameterAsync(CancellationToken cancellationToken)
    {
        return await githubCopilotTokenProvider.GetGithubCopilotTokenAsync(cancellationToken);
    }

    protected override string Scheme { get; } = "Bearer";
}