using AI.GithubCopilot.Configuration;
using MCP.Application.Interfaces;
using MCP.Domain.Interfaces;
using MCP.Infrastructure.Repositories;
using MCP.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MCP.Infrastructure.Configuration;

/// <summary>
///     Extension methods for configuring infrastructure services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Adds infrastructure services to the dependency injection container
    /// </summary>
    public static IServiceCollection AddMcpInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Register repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register JWT token service
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        // Add other infrastructure services here
        // Example: Database context, external API clients, etc.

        services.AddGithubCopilot(configuration);
        return services;
    }
}
