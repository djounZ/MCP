namespace MCP.WebApi.Extensions;

/// <summary>
/// Extension methods for configuring weather forecast endpoints
/// </summary>
public static class WeatherEndpointsExtensions
{
    /// <summary>
    /// Maps weather forecast endpoints to the application
    /// </summary>
    /// <param name="app">The web application</param>
    /// <returns>The web application for method chaining</returns>
    public static WebApplication MapWeatherEndpoints(this WebApplication app)
    {
        var summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        app.MapGet("/weatherforecast", () =>
        {
            var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
                .ToArray();
            return forecast;
        })
        .WithName("GetWeatherForecast")
        .WithSummary("Get weather forecast")
        .WithDescription("Returns a 5-day weather forecast")
        .WithOpenApi();

        return app;
    }
}

/// <summary>
/// Weather forecast record
/// </summary>
/// <param name="Date">The date of the forecast</param>
/// <param name="TemperatureC">Temperature in Celsius</param>
/// <param name="Summary">Weather summary</param>
public record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    /// <summary>
    /// Temperature in Fahrenheit
    /// </summary>
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
