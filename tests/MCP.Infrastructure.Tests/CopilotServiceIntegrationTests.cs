using Microsoft.Extensions.DependencyInjection;
using MCP.Application.Interfaces;
using MCP.Infrastructure.Services;
using Xunit.Abstractions;
using CopilotServiceOptions = MCP.Infrastructure.Options.CopilotServiceOptions;

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
    private readonly ITestOutputHelper _output;

    public CopilotServiceIntegrationTests(ITestOutputHelper output)
    {
        _output = output;
        
        // Setup dependency injection container
        var services = new ServiceCollection();

        // Register options system and CopilotServiceOptions with test or real values
        services.AddOptions();
        services.Configure<CopilotServiceOptions>(options =>
        {
            options.ClientId = "Iv1.b507a08c87ecfe98";
            options.DeviceCodeUrl = "https://github.com/login/device/code";
            options.AccessTokenUrl = "https://github.com/login/oauth/access_token";
            options.TokenUrl = "https://api.github.com/copilot_internal/v2/token";
            options.CompletionUrl = "https://copilot-proxy.githubusercontent.com/v1/engines/copilot-codex/completions";
        });

        // Register CopilotService using the same pattern as production (typed client with options)
        services.AddHttpClient<ICopilotService>(httpClient =>
            {
                // Configure HttpClient if needed
            })
            .AddTypedClient<ICopilotService>((httpClient, sp) =>
            {
                var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<CopilotServiceOptions>>().Value;
                var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<CopilotService>.Instance;
                return new CopilotService(httpClient, options, logger);
            });

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
        Assert.True(response.IsSuccess, $"Response should be successful. Error: {(response.IsFailure ? response.Error : "None")}");
        Assert.NotEmpty(response.Value);

        // Display the response in the test output
        _output.WriteLine($"Prompt: {prompt}");
        _output.WriteLine($"Response: {response.Value}");
        _output.WriteLine($"Response Length: {response.Value.Length} characters");

        // Additional assertions to ensure we got a meaningful response
        Assert.True(response.Value.Length > 10, "Response should be more than 10 characters");

        // Check if the response contains some expected keywords related to Napoleon
        var responseUpper = response.Value.ToUpperInvariant();
        var containsRelevantContent = responseUpper.Contains("NAPOLEON") ||
                                    responseUpper.Contains("EMPEROR") ||
                                    responseUpper.Contains("FRANCE") ||
                                    responseUpper.Contains("BONAPARTE");

        _output.WriteLine($"Contains relevant Napoleon content: {containsRelevantContent}");
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
        Assert.True(response.IsSuccess, $"Response should be successful. Error: {(response.IsFailure ? response.Error : "None")}");
        Assert.NotEmpty(response.Value);

        // Display the response in the test output
        _output.WriteLine($"Code Prompt: {prompt}");
        _output.WriteLine($"Code Response:");
        _output.WriteLine(response.Value);
        _output.WriteLine($"Response Length: {response.Value.Length} characters");

        // Additional assertions for code completion
        Assert.True(response.Value.Length > 5, "Code response should be more than 5 characters");
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
        // We expect the response to be successful but potentially empty for an empty prompt
        if (response.IsSuccess)
        {
            _output.WriteLine($"Empty prompt response: '{response.Value}'");
            _output.WriteLine($"Response length: {response.Value.Length}");
        }
        else
        {
            _output.WriteLine($"Empty prompt resulted in error: {response.Error}");
        }
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
