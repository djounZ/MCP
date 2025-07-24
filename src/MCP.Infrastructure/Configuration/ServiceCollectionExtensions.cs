using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Http;
using MCP.Domain.Interfaces;
using MCP.Infrastructure.Services;
using MCP.Infrastructure.Repositories;
using CopilotServiceOptions = MCP.Infrastructure.Options.CopilotServiceOptions;

namespace MCP.Infrastructure.Configuration;

/// <summary>
/// Extension methods for configuring infrastructure services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds infrastructure services to the dependency injection container
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Register repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register CopilotServiceOptions from configuration
        services.Configure<CopilotServiceOptions>(configuration.GetSection("CopilotService"));

        // Register CopilotService with options from IOptions
        services.AddHttpClient<ICopilotService, CopilotService>()
            .AddTypedClient((httpClient, sp) =>
            {
                var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<CopilotServiceOptions>>().Value;
                return new CopilotService(httpClient, options);
            });

        // Add other infrastructure services here
        // Example: Database context, external API clients, etc.

        return services;
    }
}
