using MCP.Application.Interfaces;
using MCP.Domain.Interfaces;
using MCP.Infrastructure.Repositories;
using MCP.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CopilotServiceOptions = MCP.Infrastructure.Options.CopilotServiceOptions;

namespace MCP.Infrastructure.Configuration;

/// <summary>
///     Extension methods for configuring infrastructure services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Adds infrastructure services to the dependency injection container
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Register repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register CopilotServiceOptions from configuration
        services.Configure<CopilotServiceOptions>(configuration.GetSection("CopilotService"));

        // Register CopilotService with HttpClient
        services.AddHttpClient<ICopilotService, CopilotService>();

        // Register JWT token service
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        // Add other infrastructure services here
        // Example: Database context, external API clients, etc.

        return services;
    }
}
