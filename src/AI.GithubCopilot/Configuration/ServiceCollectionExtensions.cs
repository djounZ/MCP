using System.Net;
using AI.GithubCopilot.Domain;
using AI.GithubCopilot.Infrastructure.Options;
using AI.GithubCopilot.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AI.GithubCopilot.Configuration;

/// <summary>
///     Extension methods for configuring infrastructure services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Adds infrastructure services to the dependency injection container
    /// </summary>
    public static IServiceCollection AddGithubCopilot(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<GithubOptions>(configuration.GetSection(nameof(GithubOptions)));
        services.TryAddTransient<GithubCopilotTokenAuthorizationHandler>();
        services.TryAddTransient<GithubCopilotChatAuthorizationHandler>();
        services.TryAddTransient<HttpListener>();
        services.TryAddTransient<TaskCompletionSource<bool>>();
        services.TryAddSingleton<HttpClientRunner>();
        services.TryAddSingleton<EncryptedEnvironment>();
        services.TryAddSingleton<GithubAccessTokenStore>();
        services.TryAddSingleton<GithubCopilotAccessTokenStore>();
        services.TryAddTransient<GithubAuthenticator>();
        services.AddHttpClient<GithubAccessTokenProvider>();
        services.AddHttpClient<GithubCopilotTokenProvider>()
            .AddHttpMessageHandler<GithubCopilotTokenAuthorizationHandler>();
        services.AddHttpClient<GithubCopilotChatCompletion>()
            .AddHttpMessageHandler<GithubCopilotChatAuthorizationHandler>();
        services.AddTransient<GithubCopilotChatClient>();
        return services;
    }
}
