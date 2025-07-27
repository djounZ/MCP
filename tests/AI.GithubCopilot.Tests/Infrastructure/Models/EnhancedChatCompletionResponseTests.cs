using AI.GithubCopilot.Infrastructure.Extensions;
using AI.GithubCopilot.Infrastructure.Models;
using System.Text.Json;
using Xunit;

namespace AI.GithubCopilot.Tests.Infrastructure.Models;

/// <summary>
/// Tests for enhanced ChatCompletionResponse and related models
/// </summary>
public class EnhancedChatCompletionResponseTests
{
    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        WriteIndented = true
    };

    [Fact]
    public void ChatCompletionResponse_ShouldDeserializeBasicResponse()
    {
        // Arrange
        var json = """
        {
            "id": "chatcmpl-123",
            "object": "chat.completion",
            "created": 1677652288,
            "model": "gpt-4o",
            "system_fingerprint": "fp_44709d6fcb",
            "choices": [
                {
                    "index": 0,
                    "message": {
                        "role": "assistant",
                        "content": "Hello! How can I help you today?"
                    },
                    "finish_reason": "stop"
                }
            ],
            "usage": {
                "prompt_tokens": 9,
                "completion_tokens": 9,
                "total_tokens": 18
            }
        }
        """;

        // Act
        var response = JsonSerializer.Deserialize<ChatCompletionResponse>(json, _options);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("chatcmpl-123", response.Id);
        Assert.Equal("chat.completion", response.Object);
        Assert.Equal(1677652288, response.Created);
        Assert.Equal("gpt-4o", response.Model);
        Assert.Equal("fp_44709d6fcb", response.SystemFingerprint);
        Assert.Single(response.Choices);
        
        var choice = response.Choices[0];
        Assert.Equal(0, choice.Index);
        Assert.Equal("stop", choice.FinishReason);
        Assert.NotNull(choice.Message);
        Assert.Equal("assistant", choice.Message.Role);
        Assert.Equal("Hello! How can I help you today?", choice.Message.Content?.AsText());
        
        Assert.NotNull(response.Usage);
        Assert.Equal(9, response.Usage.PromptTokens);
        Assert.Equal(9, response.Usage.CompletionTokens);
        Assert.Equal(18, response.Usage.TotalTokens);
    }

    [Fact]
    public void ChatCompletionResponse_ShouldDeserializeStreamingResponse()
    {
        // Arrange
        var json = """
        {
            "id": "chatcmpl-123",
            "object": "chat.completion.chunk",
            "created": 1677652288,
            "model": "gpt-4o",
            "choices": [
                {
                    "index": 0,
                    "delta": {
                        "role": "assistant",
                        "content": "Hello"
                    },
                    "finish_reason": null
                }
            ]
        }
        """;

        // Act
        var response = JsonSerializer.Deserialize<ChatCompletionResponse>(json, _options);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("chat.completion.chunk", response.Object);
        Assert.Single(response.Choices);
        
        var choice = response.Choices[0];
        Assert.NotNull(choice.Delta);
        Assert.Equal("assistant", choice.Delta.Role);
        Assert.Equal("Hello", choice.Delta.Content);
        Assert.Null(choice.FinishReason);
    }

    [Fact]
    public void ChatCompletionResponse_ShouldDeserializeToolCallResponse()
    {
        // Arrange
        var json = """
        {
            "id": "chatcmpl-123",
            "object": "chat.completion",
            "created": 1677652288,
            "model": "gpt-4o",
            "choices": [
                {
                    "index": 0,
                    "message": {
                        "role": "assistant",
                        "content": null,
                        "tool_calls": [
                            {
                                "id": "call_abc123",
                                "type": "function",
                                "function": {
                                    "name": "get_weather",
                                    "arguments": "{\"location\": \"Boston, MA\"}"
                                }
                            }
                        ]
                    },
                    "finish_reason": "tool_calls"
                }
            ]
        }
        """;

        // Act
        var response = JsonSerializer.Deserialize<ChatCompletionResponse>(json, _options);

        // Assert
        Assert.NotNull(response);
        var choice = response.Choices[0];
        Assert.Equal("tool_calls", choice.FinishReason);
        Assert.NotNull(choice.Message?.ToolCalls);
        Assert.Single(choice.Message.ToolCalls);
        
        var toolCall = choice.Message.ToolCalls[0];
        Assert.Equal("call_abc123", toolCall.Id);
        Assert.Equal("function", toolCall.Type);
        Assert.Equal("get_weather", toolCall.Function.Name);
        Assert.Equal("{\"location\": \"Boston, MA\"}", toolCall.Function.Arguments);
    }

    [Fact]
    public void ChatCompletionResponse_ShouldDeserializeContentFilterResponse()
    {
        // Arrange
        var json = """
        {
            "id": "chatcmpl-123",
            "object": "chat.completion",
            "created": 1677652288,
            "model": "gpt-4o",
            "choices": [
                {
                    "index": 0,
                    "message": {
                        "role": "assistant",
                        "content": "I can't help with that request."
                    },
                    "finish_reason": "content_filter",
                    "content_filter_results": {
                        "hate": {
                            "filtered": false,
                            "severity": "safe"
                        },
                        "self_harm": {
                            "filtered": true,
                            "severity": "high"
                        },
                        "sexual": {
                            "filtered": false,
                            "severity": "safe"
                        },
                        "violence": {
                            "filtered": false,
                            "severity": "safe"
                        }
                    }
                }
            ]
        }
        """;

        // Act
        var response = JsonSerializer.Deserialize<ChatCompletionResponse>(json, _options);

        // Assert
        Assert.NotNull(response);
        var choice = response.Choices[0];
        Assert.Equal("content_filter", choice.FinishReason);
        Assert.NotNull(choice.ContentFilterResults);
        
        var filters = choice.ContentFilterResults;
        Assert.False(filters.Hate.Filtered);
        Assert.Equal("safe", filters.Hate.Severity);
        Assert.True(filters.SelfHarm.Filtered);
        Assert.Equal("high", filters.SelfHarm.Severity);
    }

    [Fact]
    public void ResponseExtensions_ShouldGetContent()
    {
        // Arrange
        var response = new ChatCompletionResponse(
            Choices: new[]
            {
                new ChatChoice(
                    Index: 0,
                    Message: new ChatMessage("assistant", MessageContent.FromText("Hello world"))
                )
            },
            Created: DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Id: "test-123",
            Object: "chat.completion"
        );

        // Act
        var content = response.GetContent();

        // Assert
        Assert.Equal("Hello world", content);
    }

    [Fact]
    public void ResponseExtensions_ShouldGetToolCalls()
    {
        // Arrange
        var toolCall = new ToolCall("call_123", "function", new FunctionCall("test_func", "{}"));
        var response = new ChatCompletionResponse(
            Choices: new[]
            {
                new ChatChoice(
                    Index: 0,
                    Message: new ChatMessage("assistant", null, ToolCalls: new[] { toolCall })
                )
            },
            Created: DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Id: "test-123",
            Object: "chat.completion"
        );

        // Act
        var toolCalls = response.GetToolCalls();

        // Assert
        Assert.Single(toolCalls);
        Assert.Equal("call_123", toolCalls[0].Id);
    }

    [Fact]
    public void ResponseExtensions_ShouldCheckIfFiltered()
    {
        // Arrange
        var filteredResults = new ContentFilterResults(
            Hate: new ContentFilterResult(false, "safe"),
            SelfHarm: new ContentFilterResult(true, "high"),
            Sexual: new ContentFilterResult(false, "safe"),
            Violence: new ContentFilterResult(false, "safe")
        );

        var response = new ChatCompletionResponse(
            Choices: new[]
            {
                new ChatChoice(
                    Index: 0,
                    ContentFilterResults: filteredResults,
                    FinishReason: "content_filter"
                )
            },
            Created: DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Id: "test-123",
            Object: "chat.completion"
        );

        // Act
        var wasFiltered = response.WasFiltered();
        var isFiltered = filteredResults.IsFiltered();
        var severity = filteredResults.GetHighestFilterSeverity();

        // Assert
        Assert.True(wasFiltered);
        Assert.True(isFiltered);
        Assert.Equal("high", severity);
    }

    [Fact]
    public void ResponseExtensions_ShouldGetRefusal()
    {
        // Arrange
        var response = new ChatCompletionResponse(
            Choices: new[]
            {
                new ChatChoice(
                    Index: 0,
                    Message: new ChatMessage("assistant", null, Refusal: "I can't help with that.")
                )
            },
            Created: DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Id: "test-123",
            Object: "chat.completion"
        );

        // Act
        var refusal = response.GetRefusal();

        // Assert
        Assert.Equal("I can't help with that.", refusal);
    }

    [Fact]
    public void Usage_ShouldDeserializeWithDetails()
    {
        // Arrange
        var json = """
        {
            "prompt_tokens": 100,
            "completion_tokens": 50,
            "total_tokens": 150,
            "completion_tokens_details": {
                "reasoning_tokens": 10,
                "accepted_prediction_tokens": 5,
                "rejected_prediction_tokens": 2
            },
            "prompt_tokens_details": {
                "cached_tokens": 80
            }
        }
        """;

        // Act
        var usage = JsonSerializer.Deserialize<Usage>(json, _options);

        // Assert
        Assert.NotNull(usage);
        Assert.Equal(100, usage.PromptTokens);
        Assert.Equal(50, usage.CompletionTokens);
        Assert.Equal(150, usage.TotalTokens);
        
        Assert.NotNull(usage.CompletionTokensDetails);
        Assert.Equal(10, usage.CompletionTokensDetails.ReasoningTokens);
        Assert.Equal(5, usage.CompletionTokensDetails.AcceptedPredictionTokens);
        Assert.Equal(2, usage.CompletionTokensDetails.RejectedPredictionTokens);
        
        Assert.NotNull(usage.PromptTokensDetails);
        Assert.Equal(80, usage.PromptTokensDetails.CachedTokens);
    }
}
