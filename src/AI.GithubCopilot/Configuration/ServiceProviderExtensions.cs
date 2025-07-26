using AI.GithubCopilot.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AI.GithubCopilot.Configuration;

public static class ServiceProviderExtensions
{
    public static async Task UseGithubCopilotAsync(this IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var githubAccessTokenProvider = serviceProvider.GetRequiredService<GithubAccessTokenProvider>();
        await githubAccessTokenProvider.GetGithubAccessToken(cancellationToken);
    }
}
