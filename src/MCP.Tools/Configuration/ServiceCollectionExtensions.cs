using MCP.Tools.Infrastructure.Mappers;
using MCP.Tools.Infrastructure.Options;
using MCP.Tools.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MCP.Tools.Configuration;

public static class ServiceCollectionExtensions
{


    public static IServiceCollection AddMcpTools(this IServiceCollection services,
        IConfiguration configuration)
    {

        return services.AddInfrastructure(configuration);
    }

    private static IServiceCollection AddInfrastructure(this IServiceCollection services,
        IConfiguration configuration)
    {

        services
            .AddMappers()
            .AddOptions(configuration)
            .AddServices();
        return services;
    }

    private static IServiceCollection AddMappers(this IServiceCollection services)
    {
        services.AddSingleton<McpServerConfigurationMapper>();
        return services;
    }

    private static IServiceCollection AddOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<McpToolsOptions>(configuration.GetSection(nameof(McpToolsOptions)));
        return services;
    }

    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton<ClientTransportFactoryService>();
        services.AddSingleton<McpServerConfigurationProviderService>();
        services.AddSingleton<McpClientToolProviderService>();
        services.AddSingleton<McpClientDescriptionProviderService>();
        return services;
    }
}
