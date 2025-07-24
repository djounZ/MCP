using MCP.Infrastructure.Services;
using MCP.Domain.Interfaces;

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
        Assert.Throws<ArgumentNullException>(() => new CopilotService(null!));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void CopilotService_Constructor_WithValidHttpClient_ShouldCreateInstance()
    {
        // Arrange
        using var httpClient = new HttpClient();

        // Act
        using var service = new CopilotService(httpClient);

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
        var service = new CopilotService(httpClient);

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
        var service = new CopilotService(httpClient);

        // Act & Assert
        var exception1 = Record.Exception(() => service.Dispose());
        var exception2 = Record.Exception(() => service.Dispose());
        
        Assert.Null(exception1);
        Assert.Null(exception2);
    }
}
