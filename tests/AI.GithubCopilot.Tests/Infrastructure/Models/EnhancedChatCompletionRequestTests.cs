using AI.GithubCopilot.Infrastructure.Builders;
using AI.GithubCopilot.Infrastructure.Extensions;
using AI.GithubCopilot.Infrastructure.Models;
using System.Text.Json;
using MultipartContent = AI.GithubCopilot.Infrastructure.Models.MultipartContent;

namespace AI.GithubCopilot.Tests.Infrastructure.Models;

/// <summary>
/// Tests for enhanced ChatCompletionRequest with comprehensive features
/// </summary>
public class EnhancedChatCompletionRequestTests
{
    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        WriteIndented = true
    };

    [Fact]
    public void Builder_ShouldCreateBasicRequest()
    {
        // Arrange & Act
        var request = ChatCompletionRequestBuilder.Create()
            .WithModel("gpt-4o")
            .WithSystemMessage("You are a helpful assistant.")
            .WithUserMessage("Hello, how are you?")
            .WithTemperature(0.7)
            .WithMaxTokens(1000)
            .Build();

        // Assert
        Assert.Equal("gpt-4o", request.Model);
        Assert.Equal(2, request.Messages.Count);
        Assert.Equal("system", request.Messages[0].Role);
        Assert.Equal("user", request.Messages[1].Role);
        Assert.Equal(0.7, request.Temperature);
        Assert.Equal(1000, request.MaxTokens);
    }

    [Fact]
    public void Builder_ShouldCreateMultipartContentRequest()
    {
        // Arrange & Act
        var request = ChatCompletionRequestBuilder.Create()
            .WithModel("gpt-4o")
            .WithUserMessage(
                "What do you see in this image?".Text(),
                "https://example.com/image.jpg".ImageHighDetail()
            )
            .Build();

        // Assert
        Assert.Single(request.Messages);
        var message = request.Messages[0];
        Assert.Equal("user", message.Role);
        Assert.IsType<MultipartContent>(message.Content);
        
        var multipart = (MultipartContent)message.Content;
        Assert.Equal(2, multipart.Parts.Length);
        Assert.IsType<TextPart>(multipart.Parts[0]);
        Assert.IsType<ImagePart>(multipart.Parts[1]);
    }

    [Fact]
    public void Builder_ShouldCreateToolRequest()
    {
        // Arrange
        var weatherTool = MessageContentExtensions.CreateFunction(
            "get_weather",
            "Get current weather for a location",
            new
            {
                type = "object",
                properties = new
                {
                    location = new { type = "string", description = "City name" },
                    unit = new { type = "string", @enum = new[] { "celsius", "fahrenheit" } }
                },
                required = new[] { "location" }
            }
        );

        // Act
        var request = ChatCompletionRequestBuilder.Create()
            .WithModel("gpt-4o")
            .WithUserMessage("What's the weather in London?")
            .WithTools(weatherTool)
            .WithToolChoice(ToolChoice.Auto)
            .WithParallelToolCalls(true)
            .Build();

        // Assert
        Assert.NotNull(request.Tools);
        Assert.Single(request.Tools);
        Assert.Equal("function", request.Tools[0].Type);
        Assert.Equal("get_weather", request.Tools[0].Function.Name);
        Assert.NotNull(request.ToolChoice);
        Assert.True(request.ParallelToolCalls);
    }

    [Fact]
    public void Builder_ShouldCreateStructuredOutputRequest()
    {
        // Arrange & Act
        var request = ChatCompletionRequestBuilder.Create()
            .WithModel("gpt-4o")
            .WithUserMessage("Extract the name and age from: John is 25 years old")
            .WithJsonResponse("person_info", "Extract person information")
            .Build();

        // Assert
        Assert.NotNull(request.ResponseFormat);
        Assert.Equal("json_object", request.ResponseFormat.Type);
        Assert.NotNull(request.ResponseFormat.JsonSchema);
        Assert.Equal("person_info", request.ResponseFormat.JsonSchema.Name);
    }

    [Fact]
    public void Builder_ShouldCreateAdvancedSamplingRequest()
    {
        // Arrange & Act
        var request = ChatCompletionRequestBuilder.Create()
            .WithModel("gpt-4o")
            .WithUserMessage("Write a creative story")
            .WithTemperature(0.9)
            .WithTopP(0.95)
            .WithFrequencyPenalty(0.1)
            .WithPresencePenalty(0.1)
            .WithLogprobs(true, 5)
            .WithSeed(42)
            .WithN(3)
            .Build();

        // Assert
        Assert.Equal(0.9, request.Temperature);
        Assert.Equal(0.95, request.TopP);
        Assert.Equal(0.1, request.FrequencyPenalty);
        Assert.Equal(0.1, request.PresencePenalty);
        Assert.True(request.LogProbs);
        Assert.Equal(5, request.TopLogProbs);
        Assert.Equal(42, request.Seed);
        Assert.Equal(3, request.N);
    }

    [Fact]
    public void MessageContent_ShouldSerializeTextContent()
    {
        // Arrange
        var message = new ChatMessage("user", "Hello world");

        // Act
        var json = JsonSerializer.Serialize(message, _options);

        // Assert
        Assert.Contains("\"content\": \"Hello world\"", json);
    }

    [Fact]
    public void MessageContent_ShouldSerializeMultipartContent()
    {
        // Arrange
        var content = MessageContent.Multipart(
            new TextPart("Describe this image:"),
            new ImagePart(new ImageUrl("https://example.com/image.jpg", "high"))
        );
        var message = new ChatMessage("user", content);

        // Act
        var json = JsonSerializer.Serialize(message, _options);

        // Assert
        Assert.Contains("\"type\": \"text\"", json);
        Assert.Contains("\"type\": \"image_url\"", json);
        Assert.Contains("\"detail\": \"high\"", json);
    }

    [Fact]
    public void ToolChoice_ShouldSerializeStringValue()
    {
        // Arrange
        var choice = ToolChoice.Auto;

        // Act
        var json = JsonSerializer.Serialize(choice, _options);

        // Assert
        Assert.Equal("\"auto\"", json);
    }

    [Fact]
    public void ToolChoice_ShouldSerializeObjectValue()
    {
        // Arrange
        var choice = ToolChoice.ForFunction("my_function");

        // Act
        var json = JsonSerializer.Serialize(choice, _options);

        // Assert
        Assert.Contains("\"type\": \"function\"", json);
        Assert.Contains("\"name\": \"my_function\"", json);
    }

    [Fact]
    public void CompleteRequest_ShouldSerializeCorrectly()
    {
        // Arrange
        var request = ChatCompletionRequestBuilder.Create()
            .WithModel("gpt-4o")
            .WithSystemMessage("You are a helpful coding assistant.")
            .WithUserMessage("Help me write a function")
            .WithTemperature(0.3)
            .WithMaxTokens(500)
            .WithStream(true)
            .Build();

        // Act
        var json = JsonSerializer.Serialize(request, _options);

        // Assert
        Assert.Contains("\"model\": \"gpt-4o\"", json);
        Assert.Contains("\"temperature\": 0.3", json);
        Assert.Contains("\"max_tokens\": 500", json);
        Assert.Contains("\"stream\": true", json);
        Assert.Contains("\"messages\"", json);
    }
}
