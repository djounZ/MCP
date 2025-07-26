using AI.GithubCopilot.Configuration;
using AI.GithubCopilot.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MCP.Infrastructure.Configuration;

public static class ServiceProviderExtensions
{
    public static async Task UseMCPInfrastructureAsync(this IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        await serviceProvider.UseGithubCopilotAsync(cancellationToken);
    }
}
