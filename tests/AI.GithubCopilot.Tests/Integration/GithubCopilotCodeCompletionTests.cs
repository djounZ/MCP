using System.Text;
using AI.GithubCopilot.Infrastructure.Models;
using AI.GithubCopilot.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace AI.GithubCopilot.Tests.Integration;

/// <summary>
/// Integration tests for <see cref="GithubCopilotChatCompletion"/> class for code completion scenarios.
/// </summary>
public class GithubCopilotCodeCompletionTests : IClassFixture<TestFixture>
{
    private readonly GithubCopilotChatCompletion _copilotChatCompletion;
    private readonly ITestOutputHelper _output;

    public GithubCopilotCodeCompletionTests(TestFixture fixture, ITestOutputHelper output)
    {
        _copilotChatCompletion = fixture.ServiceProvider.GetRequiredService<GithubCopilotChatCompletion>();
        _output = output;
    }

    /// <summary>
    /// Tests that <see cref="GithubCopilotChatCompletion.GetChatCompletionStreamAsync"/> can 
    /// successfully stream code completion responses from the GitHub Copilot API.
    /// </summary>
    [Fact(Skip = "This test requires valid GitHub Copilot credentials")]
    public async Task GetChatCompletionStreamAsync_WithCodePrompt_ReturnsCodeCompletion()
    {
        // Arrange
        var messages = new List<ChatMessage>
        {
            new("system", "You are a helpful coding assistant. Only respond with code, no explanations."),
            new("user", "Create a C# function that calculates the factorial of a number using recursion.")
        };
        
        var request = new ChatCompletionRequest(
            Messages: messages,
            Model: "gpt-4",
            N: null,
            TopP: null,
            Stream: true,
            Temperature: 0.3, // Lower temperature for more deterministic responses
            MaxTokens: 500
        );

        // Act
        var responseContent = new StringBuilder();
        _output.WriteLine("Sending code completion request to GitHub Copilot API...");
        
        try
        {
            await foreach (var response in _copilotChatCompletion.GetChatCompletionStreamAsync(request, CancellationToken.None))
            {
                if (response?.Choices?.FirstOrDefault()?.Delta?.Content is { } content)
                {
                    responseContent.Append(content);
                    _output.WriteLine(content);
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
}
