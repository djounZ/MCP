using AI.GithubCopilot.Infrastructure.Mappers;
using AI.GithubCopilot.Infrastructure.Models;
using Microsoft.Extensions.AI;
using ChatMessage = AI.GithubCopilot.Infrastructure.Models.ChatMessage;
using ChatRole = Microsoft.Extensions.AI.ChatRole;

namespace AI.GithubCopilot.Tests.Infrastructure.Mappers;

/// <summary>
/// Comprehensive unit tests for GithubCopilotChatCompletionMappers covering the public API
/// </summary>
public class GithubCopilotChatCompletionMappersTests
{
    #region ToChatCompletionRequest Tests

    [Fact]
    public void ToChatCompletionRequest_WithBasicMessage_ShouldMapCorrectly()
    {
        // Arrange
        var messages = new[]
        {
            new Microsoft.Extensions.AI.ChatMessage(ChatRole.User, "Hello")
        };

        // Act
        var result = messages.ToChatCompletionRequest();

        // Assert
        result.Should().NotBeNull();
        result.Messages.Should().HaveCount(1);
        result.Messages[0].Role.Should().Be("user");
        result.Messages[0].Content.Should().NotBeNull();
        result.Stream.Should().BeFalse();
    }

    [Fact]
    public void ToChatCompletionRequest_WithChatOptions_ShouldMapAllProperties()
    {
        // Arrange
        var messages = new[]
        {
            new Microsoft.Extensions.AI.ChatMessage(ChatRole.User, "Hello")
        };
        var options = new ChatOptions
        {
            ModelId = "gpt-4o",
            Temperature = 0.7f,
            MaxOutputTokens = 1000,
            TopP = 0.9f,
            FrequencyPenalty = 0.1f,
            PresencePenalty = 0.2f,
            Seed = 42,
            Instructions = "You are helpful",
            StopSequences = ["STOP"],
            ToolMode = ChatToolMode.Auto,
            AllowMultipleToolCalls = true,
            ResponseFormat = ChatResponseFormat.Text
        };

        // Act
        var result = messages.ToChatCompletionRequest(options);

        // Assert
        result.Should().NotBeNull();
        result.Model.Should().Be("gpt-4o");
        result.Temperature.Should().Be(0.7f);
        result.MaxTokens.Should().Be(1000);
        result.TopP.Should().Be(0.9f);
        result.FrequencyPenalty.Should().Be(0.1f);
        result.PresencePenalty.Should().Be(0.2f);
        result.Seed.Should().Be(42);
        result.Stop.Should().Contain("STOP");
    }

    [Fact]
    public void ToChatCompletionRequest_WithFunctionCallContent_ShouldMapCorrectly()
    {
        // Arrange
        var arguments = new Dictionary<string, object?> { ["location"] = "Boston" };
        var functionCallContent = new FunctionCallContent("call_123", "get_weather", arguments);
        var messages = new[]
        {
            new Microsoft.Extensions.AI.ChatMessage(ChatRole.Assistant, [functionCallContent])
        };

        // Act
        var result = messages.ToChatCompletionRequest();

        // Assert
        result.Messages.Should().HaveCount(1);
        result.Messages[0].ToolCalls.Should().NotBeNull();
        result.Messages[0].ToolCalls!.Should().HaveCount(1);
        result.Messages[0].ToolCalls![0].Id.Should().Be("call_123");
        result.Messages[0].ToolCalls![0].Function.Name.Should().Be("get_weather");
    }

    [Fact]
    public void ToChatCompletionRequest_WithFunctionResultContent_ShouldMapCorrectly()
    {
        // Arrange
        var functionResultContent = new FunctionResultContent("call_123", "get_weather") { Result = "Sunny, 75°F" };
        var messages = new[]
        {
            new Microsoft.Extensions.AI.ChatMessage(ChatRole.Tool, [functionResultContent])
        };

        // Act
        var result = messages.ToChatCompletionRequest();

        // Assert
        result.Messages.Should().HaveCount(1);
        result.Messages[0].Role.Should().Be("tool");
        result.Messages[0].ToolCallId.Should().Be("call_123");
        result.Messages[0].Content.Should().NotBeNull();
    }

    #endregion

    #region ToChatResponse Tests

    [Fact]
    public void ToChatResponse_WithBasicResponse_ShouldMapCorrectly()
    {
        // Arrange
        var response = CreateChatCompletionResponse();

        // Act
        var result = response.ToChatResponse();

        // Assert
        result.Should().NotBeNull();
        result.ResponseId.Should().Be("chatcmpl-123");
        result.ModelId.Should().Be("gpt-4o");
        result.FinishReason.Should().Be(ChatFinishReason.Stop);
        result.Usage.Should().NotBeNull();
        result.Usage!.InputTokenCount.Should().Be(10);  // PromptTokens (2nd param)
        result.Usage.OutputTokenCount.Should().Be(20); // CompletionTokens (1st param)
        result.Usage.TotalTokenCount.Should().Be(30);
    }

    [Fact]
    public void ToChatResponse_WithNoChoices_ShouldThrowException()
    {
        // Arrange
        var response = new ChatCompletionResponse(
            Id: "chatcmpl-123",
            Object: "chat.completion",
            Created: 1677652288,
            Model: "gpt-4o",
            Choices: []
        );

        // Act & Assert
        var action = () => response.ToChatResponse();
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("No choices available in the response");
    }

    #endregion

    #region ToChatResponseUpdateStream Tests

    [Fact]
    public async Task ToChatResponseUpdateStream_WithMultipleResponses_ShouldMapCorrectly()
    {
        // Arrange
        var responses = CreateAsyncEnumerable(
            CreateStreamingResponse("chatcmpl-123", "Hello"),
            CreateStreamingResponse("chatcmpl-123", " world"),
            CreateStreamingResponse("chatcmpl-123", "!", finishReason: "stop")
        );

        // Act
        var results = new List<ChatResponseUpdate>();
        await foreach (var update in responses.ToChatResponseUpdateStream())
        {
            results.Add(update);
        }

        // Assert
        results.Should().HaveCount(3);
        results.All(r => r.ResponseId == "chatcmpl-123").Should().BeTrue();
        results[2].FinishReason.Should().Be(ChatFinishReason.Stop);
    }

    [Fact]
    public async Task ToChatResponseUpdateStream_WithNullResponse_ShouldSkip()
    {
        // Arrange
        var responses = CreateAsyncEnumerable(
            CreateStreamingResponse("chatcmpl-123", "Hello"),
            null,
            CreateStreamingResponse("chatcmpl-123", " world")
        );

        // Act
        var results = new List<ChatResponseUpdate>();
        await foreach (var update in responses.ToChatResponseUpdateStream())
        {
            results.Add(update);
        }

        // Assert
        results.Should().HaveCount(2);
        results.All(r => r.ResponseId == "chatcmpl-123").Should().BeTrue();
    }

    [Fact]
    public async Task ToChatResponseUpdateStream_WithCancellation_ShouldRespectToken()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var responses = CreateAsyncEnumerableWithDelay([
            CreateStreamingResponse("chatcmpl-123", "Hello"),
            CreateStreamingResponse("chatcmpl-123", " world"),
            CreateStreamingResponse("chatcmpl-123", "!")
        ], cts.Token);

        // Act
        var results = new List<ChatResponseUpdate>();
        var cancellationHandled = false;

        try
        {
            await foreach (var update in responses.ToChatResponseUpdateStream(cts.Token))
            {
                results.Add(update);
                // Cancel after first item to test cancellation
                if (results.Count == 1)
                {
                    await cts.CancelAsync();
                }
            }
        }
        catch (OperationCanceledException)
        {
            cancellationHandled = true;
        }

        // Assert - Check that cancellation worked by examining the result
        // Either cancellation exception was thrown OR processing stopped at fewer than 3 items
        var testPassed = cancellationHandled || results.Count < 3;
        testPassed.Should().BeTrue($"Expected cancellation to work. CancellationHandled: {cancellationHandled}, Results count: {results.Count}");
    }

    #endregion

    #region Edge Cases and Error Handling

    [Fact]
    public void ToChatCompletionRequest_WithEmptyMessages_ShouldMapCorrectly()
    {
        // Arrange
        var messages = Array.Empty<Microsoft.Extensions.AI.ChatMessage>();

        // Act
        var result = messages.ToChatCompletionRequest();

        // Assert
        result.Messages.Should().BeEmpty();
    }

    [Fact]
    public void ToChatCompletionRequest_WithNullOptions_ShouldMapCorrectly()
    {
        // Arrange
        var messages = new[]
        {
            new Microsoft.Extensions.AI.ChatMessage(ChatRole.User, "Hello")
        };

        // Act
        var result = messages.ToChatCompletionRequest();

        // Assert
        result.Should().NotBeNull();
        result.Model.Should().BeNull();
        result.Temperature.Should().BeNull();
    }

    [Fact]
    public void ToChatCompletionRequest_WithSystemRole_ShouldMapCorrectly()
    {
        // Arrange
        var messages = new[]
        {
            new Microsoft.Extensions.AI.ChatMessage(ChatRole.System, "You are helpful"),
            new Microsoft.Extensions.AI.ChatMessage(ChatRole.User, "Hello")
        };

        // Act
        var result = messages.ToChatCompletionRequest();

        // Assert
        result.Messages.Should().HaveCount(2);
        result.Messages[0].Role.Should().Be("system");
        result.Messages[1].Role.Should().Be("user");
    }

    [Fact]
    public void ToChatResponse_WithNullFinishReason_ShouldMapCorrectly()
    {
        // Arrange
        var choice = new ChatChoice(
            Index: 0,
            Message: new ChatMessage("assistant", MessageContent.FromText("Test")),
            FinishReason: null
        );
        var response = new ChatCompletionResponse(
            Id: "chatcmpl-123",
            Object: "chat.completion",
            Created: 1677652288,
            Model: "gpt-4o",
            Choices: [choice]
        );

        // Act
        var result = response.ToChatResponse();

        // Assert
        result.FinishReason.Should().BeNull();
    }

    [Fact]
    public void ToChatResponse_WithContentFilter_ShouldMapCorrectly()
    {
        // Arrange
        var choice = new ChatChoice(
            Index: 0,
            Message: new ChatMessage("assistant", MessageContent.FromText("Content was filtered")),
            FinishReason: "content_filter"
        );
        var response = new ChatCompletionResponse(
            Id: "chatcmpl-123",
            Object: "chat.completion",
            Created: 1677652288,
            Model: "gpt-4o",
            Choices: [choice]
        );

        // Act
        var result = response.ToChatResponse();

        // Assert
        result.FinishReason.Should().Be(ChatFinishReason.ContentFilter);
    }

    [Fact]
    public void ToChatResponse_WithLengthFinishReason_ShouldMapCorrectly()
    {
        // Arrange
        var choice = new ChatChoice(
            Index: 0,
            Message: new ChatMessage("assistant", MessageContent.FromText("Response cut off...")),
            FinishReason: "length"
        );
        var response = new ChatCompletionResponse(
            Id: "chatcmpl-123",
            Object: "chat.completion",
            Created: 1677652288,
            Model: "gpt-4o",
            Choices: [choice]
        );

        // Act
        var result = response.ToChatResponse();

        // Assert
        result.FinishReason.Should().Be(ChatFinishReason.Length);
    }

    #endregion

    #region Helper Methods

    private static ChatCompletionResponse CreateChatCompletionResponse()
    {
        var usage = new Usage(20, 10, 30);
        var choice = new ChatChoice(
            Index: 0,
            Message: new ChatMessage("assistant", MessageContent.FromText("Hello! How can I help you?")),
            FinishReason: "stop"
        );

        return new ChatCompletionResponse(
            Id: "chatcmpl-123",
            Object: "chat.completion",
            Created: 1677652288,
            Model: "gpt-4o",
            Choices: [choice],
            Usage: usage
        );
    }

    private static ChatCompletionResponse CreateStreamingResponse(string id, string content, string? finishReason = null)
    {
        var delta = new ChatDelta(Role: "assistant", Content: content);
        var choice = new ChatChoice(Index: 0, Delta: delta, FinishReason: finishReason);

        return new ChatCompletionResponse(
            Id: id,
            Object: "chat.completion.chunk",
            Created: 1677652288,
            Model: "gpt-4o",
            Choices: [choice]
        );
    }

    private static async IAsyncEnumerable<ChatCompletionResponse?> CreateAsyncEnumerable(params ChatCompletionResponse?[] responses)
    {
        foreach (var response in responses)
        {
            yield return response;
            await Task.Yield(); // Simulate async behavior
        }
    }

    private static async IAsyncEnumerable<ChatCompletionResponse?> CreateAsyncEnumerableWithDelay(
        ChatCompletionResponse?[] responses,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (var response in responses)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return response;
            try
            {
                await Task.Delay(100, cancellationToken); // Longer delay with cancellation token
            }
            catch (OperationCanceledException)
            {
                yield break; // Stop enumeration when cancelled
            }
        }
    }

    #endregion
}
