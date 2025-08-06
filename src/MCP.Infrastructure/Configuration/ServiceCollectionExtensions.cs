using AI.GithubCopilot.Configuration;
using MCP.Application.Interfaces;
using MCP.Domain.Interfaces;
using MCP.Infrastructure.Models.Mappers;
using MCP.Infrastructure.Options;
using MCP.Infrastructure.Repositories;
using MCP.Infrastructure.Services;
using MCP.Infrastructure.Services.ChatServiceImplementations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OllamaSharp;

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

        services.AddChatServices(configuration);
        return services;
    }

    private static IServiceCollection AddChatServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<ChatClientExtensionsAiAppModelsMapper>();
        services.AddCopilotChatService(configuration);
        services.AddOllamaChatService(configuration);
        services.AddScoped<ChatServiceManager>();
        services.AddScoped<AiProviderManager>();
        return services;
    }

    private static void AddOllamaChatService(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<OllamaOptions>(configuration.GetSection(nameof(OllamaOptions)));
        services.AddScoped<OllamaApiClient>(sc =>
        {
            var ollamaOptions = sc.GetRequiredService<IOptions<OllamaOptions>>().Value;

            return new OllamaApiClient(ollamaOptions.Uri, ollamaOptions.DefaultModel);
        });
        services.AddScoped<OllamaChatService>();
    }

    private static void AddCopilotChatService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddGithubCopilot(configuration);
        services.AddScoped<GithubCopilotChatService>();
    }
}
