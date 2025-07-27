using AI.GithubCopilot.Infrastructure.Models;

namespace AI.GithubCopilot.Infrastructure.Services;

public sealed class GithubAccessTokenStore(EncryptedEnvironment encryptedEnvironment)
{
    private GithubAccessTokenResponse? _githubAccessTokenResponse;

    public async Task SetAccessToken(GithubAccessTokenResponse input, CancellationToken cancellationToken)
    {
        _githubAccessTokenResponse = input;
        await encryptedEnvironment.SetEnvironmentVariableAsync(nameof(GithubAccessTokenResponse),
            _githubAccessTokenResponse, cancellationToken);
    }

    public async Task<bool> IsValidAsync(CancellationToken cancellationToken)
    {
        _githubAccessTokenResponse ??=
            await encryptedEnvironment.GetEnvironmentVariableAsync<GithubAccessTokenResponse>(
                nameof(GithubAccessTokenResponse),
                cancellationToken);
        return !string.IsNullOrEmpty(_githubAccessTokenResponse?.AccessToken);
    }

    public string AccessToken => _githubAccessTokenResponse!.AccessToken;
}
