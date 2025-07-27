namespace AI.GithubCopilot.Infrastructure.Services;

public sealed class GithubCopilotTokenAuthorizationHandler(GithubAccessTokenStore githubAccessTokenStore) : AbstractAuthorizationHandler
{
    protected override async Task<string> GetParameterAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return githubAccessTokenStore.AccessToken;
    }

    protected override string Scheme { get; } = "Token";
}
