using System.Text;
using System.Threading.RateLimiting;
using MCP.WebApi.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace MCP.WebApi.Extensions;

/// <summary>
///     Extension methods for configuring authentication and authorization
/// </summary>
public static class SecurityExtensions
{
    /// <summary>
    ///     Adds JWT Bearer authentication to the application
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration</param>
    /// <returns>The service collection for method chaining</returns>
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection("Jwt");
        var secretKey = jwtSection["SecretKey"];
        var issuer = jwtSection["Issuer"];
        var audience = jwtSection["Audience"];

        if (string.IsNullOrEmpty(secretKey))
        {
            throw new InvalidOperationException("JWT SecretKey is not configured");
        }

        var key = Encoding.ASCII.GetBytes(secretKey);

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false; // Set to true in production
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                // Custom event handlers for better logging
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                        logger.LogWarning("JWT Authentication failed: {Exception}", context.Exception.Message);
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                        logger.LogWarning("JWT Challenge triggered: {Error} - {ErrorDescription}",
                            context.Error, context.ErrorDescription);
                        return Task.CompletedTask;
                    }
                };
            });

        return services;
    }

    /// <summary>
    ///     Adds API Key authentication to the application
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration</param>
    /// <returns>The service collection for method chaining</returns>
    public static IServiceCollection AddApiKeyAuthentication(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAuthentication("ApiKey")
            .AddScheme<ApiKeyAuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(
                "ApiKey", options =>
                {
                    options.ApiKeys = configuration.GetSection("ApiKeys").Get<Dictionary<string, string>>() ??
                                      new Dictionary<string, string>();
                });

        return services;
    }

    /// <summary>
    ///     Adds authorization policies to the application
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for method chaining</returns>
    public static IServiceCollection AddCustomAuthorization(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy("RequireAdmin", policy =>
                policy.RequireClaim("role", "admin"))
            .AddPolicy("RequireUser", policy =>
                policy.RequireClaim("role", "user", "admin"))
            .AddPolicy("RequireApiKey", policy =>
                policy.RequireAuthenticatedUser()
                    .AddAuthenticationSchemes("ApiKey"));

        return services;
    }

    /// <summary>
    ///     Adds rate limiting to the application
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for method chaining</returns>
    public static IServiceCollection AddCustomRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            // Global rate limit
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    httpContext.User?.Identity?.Name ??
                    httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                    partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true, PermitLimit = 100, Window = TimeSpan.FromMinutes(1)
                    }));

            // API-specific rate limits using PartitionedRateLimiter
            options.AddPolicy("ApiLimiter", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    httpContext.User?.Identity?.Name ??
                    httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                    partition => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 5
                    }));

            // Admin endpoints with higher limits
            options.AddPolicy("AdminLimiter", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    httpContext.User?.Identity?.Name ??
                    httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                    partition => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 50,
                        Window = TimeSpan.FromMinutes(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 10
                    }));

            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.StatusCode = 429;
                await context.HttpContext.Response.WriteAsync("Too many requests. Please try again later.", token);
            };
        });

        return services;
    }
}
