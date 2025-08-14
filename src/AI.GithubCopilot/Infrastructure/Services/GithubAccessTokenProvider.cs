using AI.GithubCopilot.Infrastructure.Models;
using AI.GithubCopilot.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AI.GithubCopilot.Infrastructure.Services;

public sealed class GithubAccessTokenProvider(
    ILogger<GithubAccessTokenProvider> logger,
    HttpClient httpClient,
    IOptions<GithubOptions> options,
    GithubAccessTokenStore githubAccessTokenStore,
    HttpClientRunner httpClientRunner,
    GithubAuthenticator githubAuthenticator)
{
    private GithubOptions Options => options.Value;

    public async Task GetGithubAccessToken(
        CancellationToken cancellationToken)
    {
        if (await githubAccessTokenStore.IsValidAsync(cancellationToken))
        {
            return;
        }

        await RegisterDeviceAndGetAccessTokenAsync(cancellationToken);
    }

    private async Task RegisterDeviceAndGetAccessTokenAsync(
        CancellationToken cancellationToken)
    {
        var githubDeviceCodeResponse = await RequestDeviceCodeAsync(cancellationToken);
        await githubAuthenticator.AuthenticateAsync(githubDeviceCodeResponse, cancellationToken);
        await GetAccessTokenAsync(githubDeviceCodeResponse, cancellationToken);
    }

    private async Task<GithubDeviceCodeResponse> RequestDeviceCodeAsync(
        CancellationToken cancellationToken)
    {
        var request = new GithubDeviceCodeRequest
        {
            ClientId = Options.ClientId, Scope = Options.DeviceScope
        };

        var response =
            await httpClientRunner.SendAndDeserializeAsync<GithubDeviceCodeRequest, GithubDeviceCodeResponse>(
                request,
                httpClient,
                HttpMethod.Post,
                Options.DeviceCodeUrl,
                Options.DeviceCodeHeaders,
                HttpCompletionOption.ResponseHeadersRead,
                null,
                cancellationToken,
                logger);
        return response;
    }


    private async Task GetAccessTokenAsync(
        GithubDeviceCodeResponse githubDeviceCodeResponse,
        CancellationToken cancellationToken)
    {
        var request = new GithubAccessTokenRequest
        {
            ClientId = Options.ClientId,
            DeviceCode = githubDeviceCodeResponse.DeviceCode,
            TokenGrantType = Options.GrantType
        };

        var response =
            await httpClientRunner.SendAndDeserializeAsync<GithubAccessTokenRequest, GithubAccessTokenResponse>(
                request,
                httpClient,
                HttpMethod.Post,
                Options.TokenUrl,
                Options.TokenHeaders,
                HttpCompletionOption.ResponseHeadersRead,
                null,
                cancellationToken,
                logger);
        await githubAccessTokenStore.SetAccessToken(response, cancellationToken);
    }
}
