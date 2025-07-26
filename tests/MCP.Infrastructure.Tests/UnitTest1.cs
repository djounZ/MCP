using System.Text;
using MCP.Application.Interfaces;
using MCP.Infrastructure.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit.Abstractions;
using CopilotServiceOptions = MCP.Infrastructure.Options.CopilotServiceOptions;

namespace MCP.Infrastructure.Tests;

/// <summary>
///     Unit tests for CopilotService (isolated, no external dependencies)
/// </summary>
public class CopilotServiceUnitTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public CopilotServiceUnitTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void CopilotService_Constructor_WithNullHttpClient_ShouldThrowArgumentNullException()
    {
        // Arrange & Act & Assert
        var options = Microsoft.Extensions.Options.Options.Create(new CopilotServiceOptions());
        var logger = NullLogger<CopilotService>.Instance;
        Assert.Throws<ArgumentNullException>(() => new CopilotService(null!, options, logger, new CopilotServiceState(NullLogger<CopilotServiceState>.Instance)));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void CopilotService_Constructor_WithValidHttpClient_ShouldCreateInstance()
    {
        // Arrange
        using var httpClient = new HttpClient();
        var options = Microsoft.Extensions.Options.Options.Create(new CopilotServiceOptions());

        // Act
        var logger = NullLogger<CopilotService>.Instance;
        using var service = new CopilotService(httpClient, options, logger, new CopilotServiceState(NullLogger<CopilotServiceState>.Instance));

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
        var options = Microsoft.Extensions.Options.Options.Create(new CopilotServiceOptions());
        var logger = NullLogger<CopilotService>.Instance;
        var service = new CopilotService(httpClient, options, logger, new CopilotServiceState(NullLogger<CopilotServiceState>.Instance));

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
        var options = Microsoft.Extensions.Options.Options.Create(new CopilotServiceOptions());
        var logger = NullLogger<CopilotService>.Instance;
        var service = new CopilotService(httpClient, options, logger, new CopilotServiceState(NullLogger<CopilotServiceState>.Instance));

        // Act & Assert
        var exception1 = Record.Exception(() => service.Dispose());
        var exception2 = Record.Exception(() => service.Dispose());

        Assert.Null(exception1);
        Assert.Null(exception2);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task TestSendMessageStreamAsync()
    {

        using var client = new CopilotChatClient();

        var builder = new StringBuilder();
        await foreach (var chunk in client.SendMessageStreamAsync(
                           message: "Write a C# method that calculates fibonacci numbers recursively",
                           model: "gpt-4",
                           systemPrompt: "You are a helpful programming assistant. Provide clear, well-commented code.",
                           temperature: 0.3
                       ))
        {
            if (!string.IsNullOrEmpty(chunk.Content))
            {
                builder.Append(chunk.Content);
            }
            if (!string.IsNullOrEmpty(chunk.FinishReason))
            {
                _testOutputHelper.WriteLine($"\n\nFinished: {chunk.FinishReason}");
                _testOutputHelper.WriteLine($"Total tokens: {chunk.TotalTokens}");
                break;
            }
        }
        _testOutputHelper.WriteLine(builder.ToString());
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task GetAvailableModelsAsync()
    {

        using var client = new CopilotChatClient();// Get all available models
        var models = await client.GetAvailableModelsAsync();
        foreach (var model in models)
        {
            _testOutputHelper.WriteLine($"{model.Name} ({model.Id}) - Max Input: {model.MaxInputTokens}");
        }

// Check if a specific model is available
        bool isGpt4Available = await client.IsModelAvailableAsync("gpt-4");

// Get details about a specific model
        var modelInfo = await client.GetModelInfoAsync("gpt-4");
        if (modelInfo != null)
        {
            _testOutputHelper.WriteLine($"Model: {modelInfo.Name}, Tokenizer: {modelInfo.Tokenizer}");
        }
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task GetModelPolicyAsync()
    {var client = new CopilotChatClient();

// Get all available models (automatically enables policies where needed)
         await client.GetAvailableModelsAsync();

// Check policy status for a specific model
        var policy = await client.GetModelPolicyAsync("gpt-4");
        if (policy?.State == "enabled")
        {
            _testOutputHelper.WriteLine("Model is enabled and ready to use");
        }
    }



    [Fact]
    [Trait("Category", "Unit")]
    public async Task SendMessageAsync()
    {
        var client = new CopilotChatClient();
        var response = await client.SendMessageAsync(
        message: "Explain how dependency injection works in .NET",
        model: "gpt-4",
        systemPrompt: "You are a helpful .NET programming assistant.",
        temperature: 0.7
            );

        _testOutputHelper.WriteLine($"Response: {response.Content}");
        _testOutputHelper.WriteLine($"Tokens used: {response.TotalTokens}");

        if (response.References.Count > 0)
        {
            _testOutputHelper.WriteLine("References:");
            foreach (var reference in response.References)
            {
                _testOutputHelper.WriteLine($"  - {reference.Name}: {reference.Url}");
            }
        }
    }
}
