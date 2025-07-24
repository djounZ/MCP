using Microsoft.AspNetCore.Mvc;

namespace MCP.WebApi.Controllers;

/// <summary>
/// API controller for health checks and status information
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Basic health check endpoint
    /// </summary>
    /// <returns>Health status information</returns>
    [HttpGet]
    public IActionResult Get()
    {
        var response = new
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Version = "1.0.0",
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
        };

        return Ok(response);
    }

    /// <summary>
    /// Detailed health check with dependency status
    /// </summary>
    /// <returns>Detailed health information</returns>
    [HttpGet("detailed")]
    public IActionResult GetDetailed()
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

        return Ok(response);
    }
}
