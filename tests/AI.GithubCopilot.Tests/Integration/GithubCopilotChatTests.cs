using System.Text;
using AI.GithubCopilot.Infrastructure.Models;
using AI.GithubCopilot.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace AI.GithubCopilot.Tests.Integration;

/// <summary>
/// Integration tests for <see cref="GithubCopilotChat"/> class.
/// </summary>
public class GithubCopilotChatTests : IClassFixture<TestFixture>
{
    private readonly GithubCopilotChat _copilotChat;
    private readonly ITestOutputHelper _output;

    public GithubCopilotChatTests(TestFixture fixture, ITestOutputHelper output)
    {
        _copilotChat = fixture.ServiceProvider.GetRequiredService<GithubCopilotChat>();
        _output = output;
    }

    /// <summary>
    /// Tests that <see cref="GithubCopilotChat.GetChatCompletionStreamAsync"/> can
    /// successfully stream responses from the GitHub Copilot API.
    /// </summary>
    //[Fact(Skip = "This test requires valid GitHub Copilot credentials")]
    [Fact]
    public async Task GetChatCompletionStreamAsync_WithQuestion_ReturnsResponse()
    {
        // Arrange
        var messages = new List<ChatMessage>
        {
            new("system", "You are a helpful assistant."),
            new("user", "What is temperature in LLM?")
        };

        var request = new ChatCompletionRequest(
            Messages: messages,
            Model: "gpt-4", // Specify the model to use
            N: null,
            TopP: null,
            Stream: true,
            Temperature: 0.7,
            MaxTokens: 1000
        );

        // Act
        var responseContent = new StringBuilder();
        _output.WriteLine("Sending request to GitHub Copilot API...");

        try
        {
            await foreach (var response in _copilotChat.GetChatCompletionStreamAsync(request, CancellationToken.None))
            {
                if (response?.Choices?.FirstOrDefault()?.Delta?.Content is not { } content)
                {
                    continue;
                }

                responseContent.Append(content);
                _output.WriteLine(content);
            }

            // Assert
            var responseText = responseContent.ToString();
            _output.WriteLine($"Complete response:");
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
}
