using Microsoft.AspNetCore.Authorization;
using MCP.Application.Interfaces;
using System.Security.Claims;

namespace MCP.WebApi.Extensions;

/// <summary>
/// Extension methods for configuring authentication and authorization endpoints
/// </summary>
public static class AuthEndpointsExtensions
{
    /// <summary>
    /// Maps authentication endpoints to the application
    /// </summary>
    /// <param name="app">The web application</param>
    /// <returns>The web application for method chaining</returns>
    public static WebApplication MapAuthEndpoints(this WebApplication app)
    {
        // Login endpoint for JWT authentication
        app.MapPost("/api/auth/login", (LoginRequest loginRequest, IJwtTokenService jwtService) =>
        {
            // In a real application, validate against a user store/database
            if (IsValidUser(loginRequest.Username, loginRequest.Password))
            {
                var roles = GetUserRoles(loginRequest.Username);
                var token = jwtService.GenerateToken(loginRequest.Username, roles);
                
                var response = new LoginResponse
                {
                    Token = token,
                    ExpiresIn = 3600, // 1 hour
                    TokenType = "Bearer"
                };

                return Results.Ok(response);
            }

            return Results.Unauthorized();
        })
        .WithName("Login")
        .WithSummary("User login endpoint")
        .WithDescription("Authenticates user and returns JWT token")
        .WithOpenApi()
        .RequireRateLimiting("ApiLimiter");

        // Token validation endpoint
        app.MapGet("/api/auth/validate", (ClaimsPrincipal user) =>
        {
            var userName = user.Identity?.Name;
            var roles = user.Claims
                .Where(c => c.Type == "role")
                .Select(c => c.Value)
                .ToArray();

            var response = new
            {
                Valid = true,
                UserName = userName,
                Roles = roles,
                Claims = user.Claims.Select(c => new { c.Type, c.Value }).ToArray()
            };

            return Results.Ok(response);
        })
        .RequireAuthorization()
        .WithName("ValidateToken")
        .WithSummary("Validate current JWT token")
        .WithDescription("Returns information about the current authenticated user")
        .WithOpenApi();

        // Refresh token endpoint
        app.MapPost("/api/auth/refresh", (ClaimsPrincipal user, IJwtTokenService jwtService) =>
        {
            var userName = user.Identity?.Name;
            if (string.IsNullOrEmpty(userName))
            {
                return Results.Unauthorized();
            }

            var roles = user.Claims
                .Where(c => c.Type == "role")
                .Select(c => c.Value)
                .ToArray();

            var newToken = jwtService.GenerateToken(userName, roles);
            
            var response = new LoginResponse
            {
                Token = newToken,
                ExpiresIn = 3600,
                TokenType = "Bearer"
            };

            return Results.Ok(response);
        })
        .RequireAuthorization()
        .WithName("RefreshToken")
        .WithSummary("Refresh JWT token")
        .WithDescription("Issues a new JWT token for the authenticated user")
        .WithOpenApi()
        .RequireRateLimiting("ApiLimiter");

        return app;
    }

    /// <summary>
    /// Updates health endpoints with security
    /// </summary>
    /// <param name="app">The web application</param>
    /// <returns>The web application for method chaining</returns>
    public static WebApplication MapSecuredHealthEndpoints(this WebApplication app)
    {
        // Public basic health check (no auth required)
        app.MapGet("/api/health", () =>
        {
            var response = new
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                Version = "1.0.0",
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
            };
            return Results.Ok(response);
        })
        .WithName("GetHealth")
        .WithSummary("Basic health check endpoint")
        .WithDescription("Returns basic health status information (public endpoint)")
        .WithOpenApi()
        .RequireRateLimiting("ApiLimiter");

        // Detailed health check (requires authentication)
        app.MapGet("/api/health/detailed", (ClaimsPrincipal user) =>
        {
            var response = new
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                Version = "1.0.0",
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
                Dependencies = new
                {
                    Database = "Connected", // TODO: Add actual database health check
                    ExternalServices = "Available" // TODO: Add actual external service checks
                },
                SystemInfo = new
                {
                    MachineName = Environment.MachineName,
                    ProcessorCount = Environment.ProcessorCount,
                    WorkingSet = GC.GetTotalMemory(false)
                },
                AuthenticatedUser = user.Identity?.Name,
                UserRoles = user.Claims.Where(c => c.Type == "role").Select(c => c.Value).ToArray()
            };
            return Results.Ok(response);
        })
        .RequireAuthorization("RequireUser")
        .WithName("GetDetailedHealth")
        .WithSummary("Detailed health check with dependency status")
        .WithDescription("Returns detailed health information (requires authentication)")
        .WithOpenApi()
        .RequireRateLimiting("ApiLimiter");

        // Admin-only system metrics
        app.MapGet("/api/health/admin", () =>
        {
            var response = new
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                DetailedMetrics = new
                {
                    TotalMemory = GC.GetTotalMemory(false),
                    Gen0Collections = GC.CollectionCount(0),
                    Gen1Collections = GC.CollectionCount(1),
                    Gen2Collections = GC.CollectionCount(2),
                    WorkingSet = Environment.WorkingSet,
                    ThreadCount = Environment.ProcessorCount
                }
            };
            return Results.Ok(response);
        })
        .RequireAuthorization("RequireAdmin")
        .WithName("GetAdminHealth")
        .WithSummary("Admin-only detailed system metrics")
        .WithDescription("Returns detailed system metrics (admin only)")
        .WithOpenApi()
        .RequireRateLimiting("AdminLimiter");

        return app;
    }

    /// <summary>
    /// Validates user credentials (mock implementation)
    /// In production, this should validate against a proper user store
    /// </summary>
    private static bool IsValidUser(string username, string password)
    {
        // Mock user validation - replace with real implementation
        var validUsers = new Dictionary<string, string>
        {
            { "admin", "admin123" },
            { "user", "user123" },
            { "test", "test123" }
        };

        return validUsers.TryGetValue(username, out var validPassword) && validPassword == password;
    }

    /// <summary>
    /// Gets user roles (mock implementation)
    /// In production, this should query a proper user store
    /// </summary>
    private static string[] GetUserRoles(string username)
    {
        return username.ToLower() switch
        {
            "admin" => ["admin", "user"],
            "user" => ["user"],
            _ => ["user"]
        };
    }
}

/// <summary>
/// Login request model
/// </summary>
public record LoginRequest(string Username, string Password);

/// <summary>
/// Login response model
/// </summary>
public record LoginResponse
{
    public string Token { get; init; } = string.Empty;
    public int ExpiresIn { get; init; }
    public string TokenType { get; init; } = string.Empty;
}
