namespace MCP.WebApi.Extensions;

/// <summary>
/// Extension methods for configuring health check endpoints
/// </summary>
public static class HealthEndpointsExtensions
{
    /// <summary>
    /// Maps health check endpoints to the application
    /// </summary>
    /// <param name="app">The web application</param>
    /// <returns>The web application for method chaining</returns>
    public static WebApplication MapHealthEndpoints(this WebApplication app)
    {
        // Basic health check endpoint
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
        .WithDescription("Returns basic health status information")
        .WithOpenApi();

        // Detailed health check endpoint
        app.MapGet("/api/health/detailed", () =>
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
                }
            };
            return Results.Ok(response);
        })
        .WithName("GetDetailedHealth")
        .WithSummary("Detailed health check with dependency status")
        .WithDescription("Returns detailed health information including dependencies and system info")
        .WithOpenApi();

        return app;
    }
}
