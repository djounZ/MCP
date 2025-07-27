using System.Reflection;
using AI.GithubCopilot.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AI.GithubCopilot.Tests.Integration;

/// <summary>
/// A test fixture that provides services for integration tests.
/// </summary>
public class TestFixture : IDisposable
{
    public IServiceProvider ServiceProvider { get; }

    public TestFixture()
    {
        // Build configuration from the WebApi project
        var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ??
            throw new InvalidOperationException("Could not determine assembly location");

        var configuration = new ConfigurationBuilder()
            .SetBasePath(assemblyPath)
            .AddJsonFile(Path.Combine(GetWebApiProjectPath(), "appsettings.json"), optional: true)
            .AddEnvironmentVariables()
            .Build();

        // Create service collection
        var services = new ServiceCollection();

        // Add logging
        services.AddLogging(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Debug);
        });

        // Add GitHub Copilot services
        services.AddGithubCopilot(configuration);

        // Build service provider
        ServiceProvider = services.BuildServiceProvider();
        ServiceProvider.UseGithubCopilotAsync(CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    private string GetWebApiProjectPath()
    {
        // Navigate up to the root of the project and then to the WebApi project
        var currentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        while (currentPath != null && !Directory.Exists(Path.Combine(currentPath, "src")))
        {
            currentPath = Directory.GetParent(currentPath)?.FullName;
        }

        return Path.Combine(currentPath ?? string.Empty, "src", "MCP.WebApi");
    }

    public void Dispose()
    {
        if (ServiceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
