using AI.GithubCopilot.Infrastructure.Models;
using AI.GithubCopilot.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace AI.GithubCopilot.Infrastructure.Services;

public sealed class GithubAccessTokenProvider(
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
            ClientId = Options.ClientId, Scope = Options.Scope
        };

        var headers = new Dictionary<string, string>(Options.Headers)
        {

            [HeaderKeys.Accept] = "application/json"
        };

        var response =
            await httpClientRunner.SendAsyncAndDeserialize<GithubDeviceCodeRequest, GithubDeviceCodeResponse>(
                request,
                httpClient,
                HttpMethod.Post,
                Options.DeviceCodeUrl,
                headers,
                HttpCompletionOption.ResponseHeadersRead,
                null,
                cancellationToken);
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
            GrantType = Options.GrantType
        };


        var headers = new Dictionary<string, string>(Options.Headers)
        {

            [HeaderKeys.Accept] = "application/json"
        };
        var response =
            await httpClientRunner.SendAsyncAndDeserialize<GithubAccessTokenRequest, GithubAccessTokenResponse>(
                request,
                httpClient,
                HttpMethod.Post,
                Options.TokenUrl,
                headers,
                HttpCompletionOption.ResponseHeadersRead,
                null,
                cancellationToken);
        await githubAccessTokenStore.SetAccessToken(response, cancellationToken);
    }
}
