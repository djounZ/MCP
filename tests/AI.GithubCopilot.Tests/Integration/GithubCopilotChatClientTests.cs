using System.Text;
using System.Text.Json;
using AI.GithubCopilot.Domain;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace AI.GithubCopilot.Tests.Integration;

/// <summary>
/// Integration tests for <see cref="GithubCopilotChatClient"/> class.
/// </summary>
public class GithubCopilotChatClientTests : IClassFixture<TestFixture>
{
    private readonly GithubCopilotChatClient _copilotChatClient;
    private readonly ITestOutputHelper _output;

    public GithubCopilotChatClientTests(TestFixture fixture, ITestOutputHelper output)
    {
        _copilotChatClient = fixture.ServiceProvider.GetRequiredService<GithubCopilotChatClient>();
        _output = output;

        // Register the test output logger provider for this test
        // var loggerFactory = fixture.ServiceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>();
        // loggerFactory.AddProvider(new TestOutputLoggerProvider(_output));
    }

    /// <summary>
    /// Tests that <see cref="GithubCopilotChatClient.GetResponseAsync"/> can
    /// successfully get responses from the GitHub Copilot API.
    /// </summary>
    [Fact]
    public async Task GetResponseAsync_WithQuestion_ReturnsResponse()
    {
        // Arrange
        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, "You are a helpful assistant"),
            new(ChatRole.User, "What is temperature in LLMs? Explain it in a simple way.")
        };

        var options = new ChatOptions
        {
            ModelId = "gpt-4",
            Temperature = 0.7f,
            MaxOutputTokens = 1000
        };

        try
        {
            // Act
            var response = await _copilotChatClient.GetResponseAsync(messages, options, CancellationToken.None);

            // Assert
            var responseText = response.Messages[0].Text;
            _output.WriteLine(responseText);

            // Check if response is not empty
            Assert.NotEmpty(responseText);

            // Check if response mentions temperature
            Assert.Contains("temperature", responseText, StringComparison.OrdinalIgnoreCase);

            // Check if response explains what temperature is in LLMs
            Assert.True(
                responseText.Contains("control") ||
                responseText.Contains("parameter") ||
                responseText.Contains("setting") ||
                responseText.Contains("determines") ||
                responseText.Contains("randomness"),
                "Response should explain what temperature is in LLMs");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Error: {ex}");
            throw;
        }
    }

    /// <summary>
    /// Tests that <see cref="GithubCopilotChatClient.GetStreamingResponseAsync"/> can
    /// successfully stream responses from the GitHub Copilot API.
    /// </summary>
    [Fact]
    public async Task GetStreamingResponseAsync_WithQuestion_ReturnsResponse()
    {
        // Arrange
        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, "You are a helpful assistant"),
            new(ChatRole.User, "What is temperature in LLMs? Explain it in a simple way.")
        };

        var options = new ChatOptions
        {
            ModelId = "gpt-4",
            Temperature = 0.7f,
            MaxOutputTokens = 1000
        };

        // Act
        var responseContent = new StringBuilder();
        _output.WriteLine("Sending streaming request to GitHub Copilot API...");

        var updates = new List<ChatResponseUpdate>();
        try
        {
            await foreach (var update in _copilotChatClient.GetStreamingResponseAsync(messages, options, CancellationToken.None))
            {
                updates.Add(update);
                var textContent = update.Contents?.OfType<TextContent>().FirstOrDefault()?.Text;
                if (!string.IsNullOrEmpty(textContent))
                {
                    responseContent.Append(textContent);
                }
            }

            // Assert
            var responseText = responseContent.ToString();
            _output.WriteLine($"Complete response: {responseText}");

            // Check if response is not empty
            Assert.NotEmpty(responseText);

            // Check if response mentions temperature
            Assert.Contains("temperature", responseText, StringComparison.OrdinalIgnoreCase);

            // Check if response explains what temperature is in LLMs
            Assert.True(
                responseText.Contains("control") ||
                responseText.Contains("parameter") ||
                responseText.Contains("setting") ||
                responseText.Contains("determines") ||
                responseText.Contains("randomness"),
                "Response should explain what temperature is in LLMs");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Error: {ex}");
            throw;
        }
    }

    /// <summary>
    /// Tests that <see cref="GithubCopilotChatClient.GetStreamingResponseAsync"/> can
    /// successfully stream code completion responses from the GitHub Copilot API.
    /// </summary>
    [Fact(Skip = "This test requires valid GitHub Copilot credentials")]
    public async Task GetStreamingResponseAsync_WithCodePrompt_ReturnsCodeCompletion()
    {
        // Arrange
        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, "You are a helpful coding assistant. Only respond with code, no explanations."),
            new(ChatRole.User, "Create a C# function that calculates the factorial of a number using recursion.")
        };

        var options = new ChatOptions
        {
            ModelId = "gpt-4",
            Temperature = 0.3f, // Lower temperature for more deterministic responses
            MaxOutputTokens = 500
        };

        // Act
        var responseContent = new StringBuilder();
        _output.WriteLine("Sending code completion request to GitHub Copilot API...");

        try
        {
            await foreach (var update in _copilotChatClient.GetStreamingResponseAsync(messages, options, CancellationToken.None))
            {
                var textContent = update.Contents?.OfType<TextContent>().FirstOrDefault()?.Text;
                if (!string.IsNullOrEmpty(textContent))
                {
                    responseContent.Append(textContent);
                    _output.WriteLine(textContent);
                }
            }

            // Assert
            var responseText = responseContent.ToString();
            _output.WriteLine($"Complete response: {responseText}");

            // Check if response is not empty
            Assert.NotEmpty(responseText);

            // Check if response contains C# code indicators
            Assert.True(
                responseText.Contains("int") ||
                responseText.Contains("static") ||
                responseText.Contains("return") ||
                responseText.Contains("public") ||
                responseText.Contains("private"),
                "Response should contain C# code");

            // Check for factorial-specific content
            Assert.True(
                responseText.Contains("factorial") ||
                responseText.Contains("Factorial") ||
                responseText.Contains("*") ||
                responseText.Contains("if") ||
                responseText.Contains("return 1"),
                "Response should implement a factorial function");

            // Check for recursion
            Assert.True(
                responseText.Contains("return n * ") ||
                responseText.Contains("return n * Factorial"),
                "Response should implement factorial using recursion");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Error in code completion test: {ex}");
            throw;
        }
    }

    [Fact]
    public async Task FrontEndBackendConversation()
    {
        // Arrange
        var messagesAi1 = new List<ChatMessage>
        {
            new(ChatRole.System, "You are a back-end dotnet expert. You are talking with a front-end dotnet expert."),
            new(ChatRole.User, "Let's build an Application that relies on Model Context Protocol.")
        };

        var messagesAi2 = new List<ChatMessage>
        {
            new(ChatRole.System, "You are a front-end dotnet expert. You are talking with a back-end dotnet expert."),
        };

        var options = new ChatOptions
        {
            ModelId = "gpt-4",
            Temperature = 0.7f,
            MaxOutputTokens = 1000
        };

        for (var i = 0; i < 10; i++)
        {
            List<ChatMessage> currentUser;
            List<ChatMessage> currentAssistant;
            string reply;
            if (i % 2 == 0)
            {
                currentUser = messagesAi1;
                currentAssistant = messagesAi2;
                reply = "Backend Expert";
            }
            else
            {
                currentUser = messagesAi2;
                currentAssistant = messagesAi1;
                reply = "Frontend Expert";
            }

            _output.WriteLine("////////////////////////////////////////////////////////////////////////////////");
            _output.WriteLine($"Round #{i} Response from {reply}:");
            _output.WriteLine("////////////////////////////////////////////////////////////////////////////////");

            var responseContent = new StringBuilder();

            await foreach (var update in _copilotChatClient.GetStreamingResponseAsync(currentUser, options, CancellationToken.None))
            {
                var textContent = update.Contents?.OfType<TextContent>().FirstOrDefault()?.Text;
                if (!string.IsNullOrEmpty(textContent))
                {
                    responseContent.Append(textContent);
                }
            }

            var responseText = responseContent.ToString();
            _output.WriteLine(responseText);
            currentUser.Add(new ChatMessage(ChatRole.Assistant, responseText));
            currentAssistant.Add(new ChatMessage(ChatRole.User, responseText));
        }
    }
}
