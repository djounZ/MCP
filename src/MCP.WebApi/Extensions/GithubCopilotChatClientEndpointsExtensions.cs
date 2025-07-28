using Microsoft.Extensions.AI;
using AI.GithubCopilot.Domain;

namespace MCP.WebApi.Extensions;

/// <summary>
/// Extension methods for configuring GitHub Copilot chat client endpoints
/// </summary>
public static class GithubCopilotChatClientEndpointsExtensions
{
    /// <summary>
    /// Maps GitHub Copilot chat client endpoints to the application
    /// </summary>
    /// <param name="app">The web application</param>
    /// <returns>The web application for method chaining</returns>
    public static WebApplication MapGithubCopilotChatEndpoints(this WebApplication app)
    {
        // Non-streaming chat completion endpoint
        app.MapPost("/api/chat/completions", async (
            ChatRequest request,
            GithubCopilotChatClient chatClient,
            CancellationToken cancellationToken) =>
            {
                var response = await chatClient.GetResponseAsync(request.Messages, request.Options, cancellationToken);
                return Results.Ok(response);
            })
            .WithName("CreateChatCompletion")
            .WithSummary("Create a chat completion")
            .WithDescription("Creates a non-streaming chat completion using GitHub Copilot")
            .WithOpenApi();

        // Streaming chat completion endpoint - returns IAsyncEnumerable<ChatResponseUpdate>
        app.MapPost("/api/chat/completions/stream", (
            ChatRequest request,
            GithubCopilotChatClient chatClient,
            CancellationToken cancellationToken) => chatClient.GetStreamingResponseAsync(request.Messages, request.Options, cancellationToken))
            .WithName("CreateStreamingChatCompletion")
            .WithSummary("Create a streaming chat completion")
            .WithDescription("Creates a streaming chat completion using GitHub Copilot")
            .WithOpenApi();

        // Simple non-streaming chat endpoint
        app.MapPost("/api/chat", async (
            SimpleChatRequest request,
            GithubCopilotChatClient chatClient,
            CancellationToken cancellationToken) =>
            {
                var messages = new List<ChatMessage>
                {
                    new(ChatRole.System, request.SystemPrompt ?? "You are a helpful assistant."),
                    new(ChatRole.User, request.Message)
                };

                var options = new ChatOptions
                {
                    ModelId = request.Model ?? "gpt-4",
                    Temperature = request.Temperature ?? 0.7f,
                    MaxOutputTokens = request.MaxTokens ?? 1000
                };

                var response = await chatClient.GetResponseAsync(messages, options, cancellationToken);
                return Results.Ok(response);
            })
            .WithName("SimpleChat")
            .WithSummary("Simple chat endpoint")
            .WithDescription("Send a simple message and get a non-streaming response from GitHub Copilot")
            .WithOpenApi();

        // Simple streaming chat endpoint - returns IAsyncEnumerable<ChatResponseUpdate>
        app.MapPost("/api/chat/stream", (
            SimpleChatRequest request,
            GithubCopilotChatClient chatClient,
            CancellationToken cancellationToken) =>
            {
                var messages = new List<ChatMessage>
                {
                    new(ChatRole.System, request.SystemPrompt ?? "You are a helpful assistant."),
                    new(ChatRole.User, request.Message)
                };

                var options = new ChatOptions
                {
                    ModelId = request.Model ?? "gpt-4",
                    Temperature = request.Temperature ?? 0.7f,
                    MaxOutputTokens = request.MaxTokens ?? 1000
                };

                return chatClient.GetStreamingResponseAsync(messages, options, cancellationToken);
            })
            .WithName("SimpleChatStream")
            .WithSummary("Simple streaming chat endpoint")
            .WithDescription("Send a simple message and get a streaming response from GitHub Copilot")
            .WithOpenApi();

        return app;
    }
}

/// <summary>
/// Chat request using Microsoft.Extensions.AI types
/// </summary>
/// <param name="Messages">List of chat messages</param>
/// <param name="Options">Chat options</param>
public record ChatRequest(
    IEnumerable<ChatMessage> Messages,
    ChatOptions? Options = null
);

/// <summary>
/// Simple chat request model
/// </summary>
/// <param name="Message">The user message</param>
/// <param name="SystemPrompt">Optional system prompt</param>
/// <param name="Model">The model to use</param>
/// <param name="Temperature">Sampling temperature</param>
/// <param name="MaxTokens">Maximum tokens to generate</param>
public record SimpleChatRequest(
    string Message,
    string? SystemPrompt = null,
    string? Model = null,
    float? Temperature = null,
    int? MaxTokens = null
);
