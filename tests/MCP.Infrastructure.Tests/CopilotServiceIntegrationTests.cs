using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using MCP.Domain.Interfaces;
using MCP.Infrastructure.Services;

namespace MCP.Infrastructure.Tests.Integration;

/// <summary>
/// Integration tests for CopilotService
/// Note: These tests require actual GitHub Copilot authentication and network access
/// They should be run separately from unit tests
/// </summary>
public class CopilotServiceIntegrationTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly ICopilotService _copilotService;

    public CopilotServiceIntegrationTests()
    {
        // Setup dependency injection container
        var services = new ServiceCollection();
        
        // Register HttpClient and CopilotService
        services.AddHttpClient<ICopilotService, CopilotService>();
        
        _serviceProvider = services.BuildServiceProvider();
        _copilotService = _serviceProvider.GetRequiredService<ICopilotService>();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetCompletionAsync_WithNapoleonPrompt_ShouldReturnNonEmptyResponse()
    {
        // Arrange
        const string prompt = "Hello, who was Napoleon?";
        const string language = "text";

        // Act
        var response = await _copilotService.GetCompletionAsync(prompt, language);

        // Assert
        Assert.NotNull(response);
        Assert.NotEmpty(response);
        
        // Display the response in the test output
        Console.WriteLine($"Prompt: {prompt}");
        Console.WriteLine($"Response: {response}");
        Console.WriteLine($"Response Length: {response.Length} characters");
        
        // Additional assertions to ensure we got a meaningful response
        Assert.True(response.Length > 10, "Response should be more than 10 characters");
        
        // Check if the response contains some expected keywords related to Napoleon
        var responseUpper = response.ToUpperInvariant();
        var containsRelevantContent = responseUpper.Contains("NAPOLEON") || 
                                    responseUpper.Contains("EMPEROR") || 
                                    responseUpper.Contains("FRANCE") ||
                                    responseUpper.Contains("BONAPARTE");
        
        Console.WriteLine($"Contains relevant Napoleon content: {containsRelevantContent}");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetCompletionAsync_WithCodePrompt_ShouldReturnCodeCompletion()
    {
        // Arrange
        const string prompt = "def fibonacci(n):";
        const string language = "python";

        // Act
        var response = await _copilotService.GetCompletionAsync(prompt, language);

        // Assert
        Assert.NotNull(response);
        Assert.NotEmpty(response);
        
        // Display the response in the test output
        Console.WriteLine($"Code Prompt: {prompt}");
        Console.WriteLine($"Code Response:");
        Console.WriteLine(response);
        Console.WriteLine($"Response Length: {response.Length} characters");
        
        // Additional assertions for code completion
        Assert.True(response.Length > 5, "Code response should be more than 5 characters");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetCompletionAsync_WithEmptyPrompt_ShouldHandleGracefully()
    {
        // Arrange
        const string prompt = "";

        // Act
        var response = await _copilotService.GetCompletionAsync(prompt);

        // Assert
        Assert.NotNull(response);
        Console.WriteLine($"Empty prompt response: '{response}'");
        Console.WriteLine($"Response length: {response.Length}");
    }

    public void Dispose()
    {
        if (_copilotService is IDisposable disposableService)
        {
            disposableService.Dispose();
        }
        _serviceProvider?.Dispose();
    }
}
