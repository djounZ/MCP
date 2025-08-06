using AI.GithubCopilot.Infrastructure.Models;

namespace AI.GithubCopilot.Infrastructure.Services;

public sealed class GithubCopilotAccessTokenStore
{
    private record CachedToken(GithubCopilotAccessTokenResponse GithubCopilotAccessTokenResponse, DateTimeOffset ExpiresAt);
    private CachedToken? _cachedToken;
    public void SetToken(GithubCopilotAccessTokenResponse tokenResponse)
    {
        var expiresAt = DateTimeOffset.FromUnixTimeSeconds(tokenResponse.ExpiresAt);
        _cachedToken = new CachedToken(tokenResponse, expiresAt);
    }

    public bool IsValid =>
        _cachedToken != null && _cachedToken.ExpiresAt > DateTimeOffset.UtcNow.AddMinutes(5);


    public string Token => _cachedToken?.GithubCopilotAccessTokenResponse.Token!;
}
