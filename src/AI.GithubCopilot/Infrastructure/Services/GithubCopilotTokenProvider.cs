using System.Text.Json;
using System.Text.Json.Serialization;
using AI.GithubCopilot.Infrastructure.Models;
using AI.GithubCopilot.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AI.GithubCopilot.Infrastructure.Services;

public sealed class GithubCopilotTokenProvider(
    ILogger<GithubCopilotTokenProvider> logger,
    HttpClient httpClient,
    IOptions<GithubOptions> options,
    GithubCopilotAccessTokenStore githubCopilotAccessTokenStore,
    HttpClientRunner httpClientRunner)
{
    private GithubOptions Options => options.Value;
    private readonly SemaphoreSlim _tokenSemaphore = new(1, 1);


    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Gets a valid Copilot Bearer token, refreshing if necessary
    /// </summary>
    public async Task<string> GetGithubCopilotTokenAsync(CancellationToken cancellationToken)
    {
        // Check if cached token is still valid
        if (githubCopilotAccessTokenStore.IsValid)
        {
            return githubCopilotAccessTokenStore.Token;
        }

        await _tokenSemaphore.WaitAsync(cancellationToken);
        try
        {

            var tokenResponse =
                await httpClientRunner.SendAndDeserializeAsync<GithubCopilotAccessTokenResponse>(
                    httpClient,
                    HttpMethod.Get,
                    Options.CopilotTokenUrl,
                    Options.CopilotTokenHeaders,
                    HttpCompletionOption.ResponseHeadersRead,
                    JsonOptions,
                    cancellationToken,
                    logger);

            githubCopilotAccessTokenStore.SetToken(tokenResponse);
            return githubCopilotAccessTokenStore.Token;
        }
        finally
        {
            _tokenSemaphore.Release();
        }
    }
}
