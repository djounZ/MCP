using Microsoft.Extensions.AI;
using AI.GithubCopilot.Domain;
using MCP.Application.DTOs.AI.ChatCompletion;
using MCP.Application.DTOs.AI.Contents;
using MCP.Infrastructure.Services;

namespace MCP.WebApi.Extensions;

/// <summary>
/// Extension methods for configuring GitHub Copilot chat client endpoints
/// </summary>
public static class GithubCopilotChatClientEndpointsExtensions
{
    private const string StreamingContentType = "text/event-stream";
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
            GithubCopilotChatService chatClient,
            CancellationToken cancellationToken) =>
            {
                var response = await chatClient.GetResponseAsync(request.Messages, request.Options, cancellationToken);
                return Results.Ok(response);
            })
            .WithName("CreateChatCompletion")
            .WithSummary("Create a chat completion")
            .WithDescription("Creates a non-streaming chat completion using GitHub Copilot")
            .WithTags("GithubCopilotChat")
            .Produces<ChatResponseAppModel>()
            .WithOpenApi();

        // Streaming chat completion endpoint - returns IAsyncEnumerable<ChatResponseUpdate>
        app.MapPost("/api/chat/completions/stream", (
            ChatRequest request,
            OllamaChatService chatClient,
            CancellationToken cancellationToken) =>
            {
                var streamingResponseAsync = chatClient.GetStreamingResponseAsync(request.Messages, request.Options, cancellationToken);
                return Results.Stream(
                    async (stream) =>
                    {
                        await foreach (var update in streamingResponseAsync)
                        {
                            var data = System.Text.Json.JsonSerializer.Serialize(update);
                            var bytes = System.Text.Encoding.UTF8.GetBytes(data + Environment.NewLine);
                            await stream.WriteAsync(bytes, 0, bytes.Length, cancellationToken);
                            await stream.FlushAsync(cancellationToken);
                        }
                    },
                    contentType: StreamingContentType
                );
            })
            .WithName("CreateStreamingChatCompletion")
            .WithSummary("Create a streaming chat completion")
            .WithDescription("Creates a streaming chat completion using GitHub Copilot")
            .WithTags("GithubCopilotChat")
            .Produces<ChatResponseUpdateAppModel[]>()
            .WithOpenApi();

        // Simple non-streaming chat endpoint
        app.MapPost("/api/chat", async (
            SimpleChatRequest request,
            GithubCopilotChatService chatClient,
            CancellationToken cancellationToken) =>
            {
                var messages = new List<ChatMessageAppModel>
                {
                    new(ChatRoleEnumAppModel.System, [new TextContentAppModel([], request.SystemPrompt)] ),
                    new(ChatRoleEnumAppModel.User, [new TextContentAppModel([], request.Message)] )
                };

                var options = new ChatOptionsAppModel(
                    null,
                    null,
                    request.Temperature ?? 0.7f,
                    request.MaxTokens ?? 1000,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    request.Model ?? "gpt-4",
                    null,
                    null,
                    null
                    );

                var response = await chatClient.GetResponseAsync(messages, options, cancellationToken);
                return Results.Ok(response);
            })
            .WithName("SimpleChat")
            .WithSummary("Simple chat endpoint")
            .WithDescription("Send a simple message and get a non-streaming response from GitHub Copilot")
            .WithTags("GithubCopilotChat")
            .Produces<ChatResponseAppModel>()
            .WithOpenApi();

        // Simple streaming chat endpoint - returns IAsyncEnumerable<ChatResponseUpdate>
        app.MapPost("/api/chat/stream", (
            SimpleChatRequest request,
            GithubCopilotChatService chatClient,
            CancellationToken cancellationToken) =>
            {

                var messages = new List<ChatMessageAppModel>
                {
                    new(ChatRoleEnumAppModel.System, [new TextContentAppModel([], request.SystemPrompt)] ),
                    new(ChatRoleEnumAppModel.User, [new TextContentAppModel([], request.Message)] )
                };

                var options = new ChatOptionsAppModel(
                    null,
                    null,
                    request.Temperature ?? 0.7f,
                    request.MaxTokens ?? 1000,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    request.Model ?? "gpt-4",
                    null,
                    null,
                    null
                );

                var responseStream = chatClient.GetStreamingResponseAsync(messages, options, cancellationToken);
                return responseStream.ToResult(cancellationToken);
            })
            .WithName("SimpleChatStream")
            .WithSummary("Simple streaming chat endpoint")
            .WithDescription("Send a simple message and get a streaming response from GitHub Copilot")
            .WithTags("GithubCopilotChat")
            .Produces<ChatResponseUpdateAppModel[]>()
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
    IEnumerable<ChatMessageAppModel> Messages,
    ChatOptionsAppModel? Options = null
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
    string SystemPrompt,
    string? Model = null,
    float? Temperature = null,
    int? MaxTokens = null
);
