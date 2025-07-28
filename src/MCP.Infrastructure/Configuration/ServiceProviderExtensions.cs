using AI.GithubCopilot.Configuration;

namespace MCP.Infrastructure.Configuration;

public static class ServiceProviderExtensions
{
    public static async Task UseMcpInfrastructureAsync(this IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        await serviceProvider.UseGithubCopilotAsync(cancellationToken);
    }
}
