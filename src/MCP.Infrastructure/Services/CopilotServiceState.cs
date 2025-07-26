using MCP.Domain.Common;
using MCP.Infrastructure.Models;
using Microsoft.Extensions.Logging;

namespace MCP.Infrastructure.Services;

public sealed class CopilotServiceState
{
    public GithubDeviceCodeResponse GithubDeviceCodeResponse { get; set; } = new ();
    public GithubAccessTokenResponse GithubAccessTokenResponse { get; private set; }

    private readonly ILogger<CopilotServiceState> _logger;
    public CopilotServiceState(ILogger<CopilotServiceState> logger)
    {
        _logger = logger;
        GithubAccessTokenResponse=EnvHelpers.GetEnvironmentVariableAsync<GithubAccessTokenResponse>(nameof(GithubAccessTokenResponse),_logger).ConfigureAwait(false).GetAwaiter().GetResult() ?? new GithubAccessTokenResponse();
    }


    public async Task< Result<Unit>> SetAccessToken(GithubAccessTokenResponse input)
    {
        GithubAccessTokenResponse = input;
        var environmentVariable = await EnvHelpers.SetEnvironmentVariable(nameof(GithubAccessTokenResponse), GithubAccessTokenResponse, _logger);
        return environmentVariable ? Result<Unit>.Success(Unit.Value) : Result<Unit>.Failure("");
    }


    public bool IsValid => !string.IsNullOrEmpty(GithubAccessTokenResponse.AccessToken);
}
