using System.Net.Http.Headers;

namespace AI.GithubCopilot.Infrastructure.Services;

public abstract class AbstractAuthorizationHandler : DelegatingHandler
{
    protected abstract Task<string> GetParameterAsync(
        CancellationToken cancellationToken);
    protected abstract string Scheme { get; }
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {

        request.Headers.Authorization = new AuthenticationHeaderValue(Scheme, await GetParameterAsync(cancellationToken));
        // Call the inner handler
        return await base.SendAsync(request, cancellationToken);
    }
}
