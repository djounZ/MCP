using MCP.Infrastructure.Services;
using MCP.Application.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using CopilotServiceOptions = MCP.Infrastructure.Options.CopilotServiceOptions;

namespace MCP.Infrastructure.Tests.Unit;

/// <summary>
/// Unit tests for CopilotService (isolated, no external dependencies)
/// </summary>
public class CopilotServiceUnitTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public void CopilotService_Constructor_WithNullHttpClient_ShouldThrowArgumentNullException()
    {
        // Arrange & Act & Assert
        var options = new CopilotServiceOptions();
        var logger = NullLogger<CopilotService>.Instance;
        Assert.Throws<ArgumentNullException>(() => new CopilotService(null!, options, logger));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void CopilotService_Constructor_WithValidHttpClient_ShouldCreateInstance()
    {
        // Arrange
        using var httpClient = new HttpClient();
        var options = new CopilotServiceOptions();

        // Act
        var logger = NullLogger<CopilotService>.Instance;
        using var service = new CopilotService(httpClient, options, logger);

        // Assert
        Assert.NotNull(service);
        Assert.IsAssignableFrom<ICopilotService>(service);
        Assert.IsAssignableFrom<IDisposable>(service);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void CopilotService_Dispose_ShouldNotThrow()
    {
        // Arrange
        using var httpClient = new HttpClient();
        var options = new CopilotServiceOptions();
        var logger = NullLogger<CopilotService>.Instance;
        var service = new CopilotService(httpClient, options, logger);

        // Act & Assert
        var exception = Record.Exception(() => service.Dispose());
        Assert.Null(exception);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void CopilotService_Dispose_CalledMultipleTimes_ShouldNotThrow()
    {
        // Arrange
        using var httpClient = new HttpClient();
        var options = new CopilotServiceOptions();
        var logger = NullLogger<CopilotService>.Instance;
        var service = new CopilotService(httpClient, options, logger);

        // Act & Assert
        var exception1 = Record.Exception(() => service.Dispose());
        var exception2 = Record.Exception(() => service.Dispose());
        
        Assert.Null(exception1);
        Assert.Null(exception2);
    }
}
