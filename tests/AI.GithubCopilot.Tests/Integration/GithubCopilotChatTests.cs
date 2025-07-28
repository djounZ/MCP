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

        // Register the test output logger provider for this test
        var loggerFactory = fixture.ServiceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>();
        loggerFactory.AddProvider(new TestOutputLoggerProvider(_output));
    }
    //[Fact(Skip = "This test requires valid GitHub Copilot credentials")]
    [Fact]
    public async Task GetChatCompletionAsync_WithQuestion_ReturnsResponse()
    {
        // Arrange
        var messages = new List<ChatMessage>
        {
            new("system", "You are an helpful assistant"),
            new("user", "What is temperature in LLMs? Explain it in a simple way.")
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

        try
        {
            var chatCompletionAsync = await _copilotChat.GetChatCompletionAsync(request, CancellationToken.None);
           // _output.WriteLine($"Complete response:");
            _output.WriteLine(chatCompletionAsync.Choices?[0].Message?.Content?.AsText());
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Error: {ex}");
            throw;
        }
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
            new("system", "You are an helpful assistant"),
            new("user", "What is temperature in LLMs? Explain it in a simple way.")
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
        //_output.WriteLine("Sending request to GitHub Copilot API...");

        var chatCompletionResponses = new List<ChatCompletionResponse?>();
        try
        {
            await foreach (var response in _copilotChat.GetChatCompletionStreamAsync(request, CancellationToken.None))
            {
                chatCompletionResponses.Add(response);
                if (response?.Choices?.FirstOrDefault()?.Delta?.Content is not { } content)
                {
                    continue;
                }

                responseContent.Append(content);
                //_output.WriteLine(response.ToString());
            }

            // Assert
            var responseText = responseContent.ToString();
           // _output.WriteLine($"Complete response:");
            _output.WriteLine(responseText);

            // Check if response is not empty
            // Assert.NotEmpty(responseText);
            //
            // // Check if response mentions temperature
            // Assert.Contains("temperature", responseText, StringComparison.OrdinalIgnoreCase);
            //
            // // Check if response explains what temperature is in LLMs
            // Assert.True(
            //     responseText.Contains("control") ||
            //     responseText.Contains("parameter") ||
            //     responseText.Contains("setting") ||
            //     responseText.Contains("determines") ||
            //     responseText.Contains("randomness"),
            //     "Response should explain what temperature is in LLMs");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Error: {ex}");
            throw;
        }
    }


    [Fact]
    public async Task FrontEndBackendConversation()
    {
        var system = "system";
        var assistant = "assistant";
        var user = "user";
        // Arrange
        var messagesAi1 = new List<ChatMessage>
        {
            new(system, "Your are back-end dotnet expert. You are talking with front-end dotnet expert."),
            new(user, "Lets build an Application that relies on Model Context Protocol.")
        };

        var messagesAi2 = new List<ChatMessage>
        {
            new(system, "Your are front-end dotnet expert. You are talking with back-end dotnet expert."),
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

            var request = new ChatCompletionRequest(
                Messages: currentUser,
                Model: "gpt-4", // Specify the model to use
                N: null,
                TopP: null,
                Stream: true,
                Temperature: 0.7,
                MaxTokens: 1000
            );

            var responseContent = new StringBuilder();

            await foreach (var response in _copilotChat.GetChatCompletionStreamAsync(request, CancellationToken.None))
            {
                if (response?.Choices.FirstOrDefault()?.Delta?.Content is not { } content)
                {
                    continue;
                }

                responseContent.Append(content);
            }

            var responseText = responseContent.ToString();
            _output.WriteLine(responseText);
            currentUser.Add(new ChatMessage(assistant, responseText));
            currentAssistant.Add(new ChatMessage(user, responseText));
        }

    }
}
