using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Http;
using MCP.Domain.Interfaces;
using MCP.Infrastructure.Services;
using MCP.Infrastructure.Repositories;

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

        // Register external services
        services.AddHttpClient<ICopilotService, CopilotService>();

        // Add other infrastructure services here
        // Example: Database context, external API clients, etc.

        return services;
    }
}
