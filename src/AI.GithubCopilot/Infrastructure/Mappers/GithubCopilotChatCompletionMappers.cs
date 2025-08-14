using AI.GithubCopilot.Infrastructure.Models;
using Microsoft.Extensions.AI;
using System.Text.Json;
using ChatMessage = AI.GithubCopilot.Infrastructure.Models.ChatMessage;
using ChatRole = Microsoft.Extensions.AI.ChatRole;
using AITextContent = Microsoft.Extensions.AI.TextContent;

namespace AI.GithubCopilot.Infrastructure.Mappers;

public static class GithubCopilotChatCompletionMappers
{
    /// <summary>
    /// Converts Microsoft.Extensions.AI chat messages and options to a GitHub Copilot ChatCompletionRequest
    /// </summary>
    public static ChatCompletionRequest ToChatCompletionRequest(this IEnumerable<Microsoft.Extensions.AI.ChatMessage> messages, ChatOptions? options = null)
    {
        return ToChatCompletionRequest(messages, options, stream: false);
    }

    /// <summary>
    /// Converts Microsoft.Extensions.AI chat messages and options to a GitHub Copilot ChatCompletionRequest with streaming option
    /// </summary>
    private static ChatCompletionRequest ToChatCompletionRequest(this IEnumerable<Microsoft.Extensions.AI.ChatMessage> messages, ChatOptions? options, bool stream)
    {
        var copilotMessages = ToCopilotChatMessages(messages, options).ToList();

        return new ChatCompletionRequest(
            Messages: copilotMessages,
            Model: options?.ModelId,
            Temperature: options?.Temperature,
            MaxTokens: options?.MaxOutputTokens,
            Stream: stream,
            TopP: options?.TopP,
            FrequencyPenalty: options?.FrequencyPenalty,
            PresencePenalty: options?.PresencePenalty,
            Stop: options?.StopSequences?.ToList(),
            Seed: (int?)options?.Seed,
            Tools: ToCopilotTools(options?.Tools),
            ToolChoice: ToCopilotToolChoice(options?.ToolMode),
            ParallelToolCalls: options?.AllowMultipleToolCalls,
            ResponseFormat: ToCopilotResponseFormat(options?.ResponseFormat)
        );
    }

    /// <summary>
    /// Converts a GitHub Copilot ChatCompletionResponse to Microsoft.Extensions.AI ChatResponse
    /// </summary>
    public static ChatResponse ToChatResponse(this ChatCompletionResponse response)
    {
        var choice = response.Choices.FirstOrDefault();
        if (choice == null)
        {
            throw new InvalidOperationException("No choices available in the response");
        }

        var message = ToAiChatMessage(choice);
        var chatResponse = new ChatResponse(message)
        {
            ResponseId = response.Id,
            ModelId = response.Model,
            CreatedAt = DateTimeOffset.FromUnixTimeSeconds(response.Created),
            FinishReason = ToAiFinishReason(choice.FinishReason),
            RawRepresentation = response
        };

        if (response.Usage != null)
        {
            chatResponse.Usage = new UsageDetails
            {
                InputTokenCount = response.Usage.PromptTokens,
                OutputTokenCount = response.Usage.CompletionTokens,
                TotalTokenCount = response.Usage.TotalTokens
            };
        }

        return chatResponse;
    }

    /// <summary>
    /// Converts a GitHub Copilot ChatCompletionResponse streaming chunk to Microsoft.Extensions.AI ChatResponseUpdate
    /// This overload is specifically designed for processing individual streaming chunks
    /// </summary>
    private static ChatResponseUpdate ToChatResponseUpdate(this ChatCompletionResponse response, string? responseId)
    {
        var choice = response.Choices.FirstOrDefault();
        var contents = new List<AIContent>();

        // For streaming chunks, prioritize delta content over message content
        if (choice?.Delta != null)
        {
            // Add delta text content
            if (!string.IsNullOrEmpty(choice.Delta.Content))
            {
                contents.Add(new AITextContent(choice.Delta.Content));
            }

            // Add delta tool calls
            if (choice.Delta.ToolCalls != null)
            {
                foreach (var toolCall in choice.Delta.ToolCalls)
                {
                    if (toolCall.Function?.Name != null)
                    {
                        var arguments = ParseArguments(toolCall.Function.Arguments);
                        contents.Add(new FunctionCallContent(
                            toolCall.Id ?? Guid.NewGuid().ToString(),
                            toolCall.Function.Name,
                            arguments
                        ));
                    }
                }
            }
        }
        // Fallback to message content if no delta
        else if (choice?.Message != null)
        {
            if (choice.Message.Content?.AsText() is { } text && !string.IsNullOrEmpty(text))
            {
                contents.Add(new AITextContent(text));
            }

            if (choice.Message.ToolCalls != null)
            {
                foreach (var toolCall in choice.Message.ToolCalls)
                {
                    var arguments = ParseArguments(toolCall.Function.Arguments);
                    contents.Add(new FunctionCallContent(
                        toolCall.Id,
                        toolCall.Function.Name,
                        arguments
                    ));
                }
            }
        }

        // Add usage information if available (typically only in the final chunk)
        if (response.Usage != null)
        {
            contents.Add(new UsageContent(new UsageDetails
            {
                InputTokenCount = response.Usage.PromptTokens,
                OutputTokenCount = response.Usage.CompletionTokens,
                TotalTokenCount = response.Usage.TotalTokens
            })
            {
                RawRepresentation = response.Usage
            });
        }

        var role = ExtractRoleFromChoice(choice);

        var update = new ChatResponseUpdate(role, contents)
        {
            ResponseId = responseId ?? response.Id,
            MessageId = responseId ?? response.Id,
            ModelId = response.Model,
            CreatedAt = DateTimeOffset.FromUnixTimeSeconds(response.Created),
            FinishReason = ToAiFinishReason(choice?.FinishReason),
            RawRepresentation = response
        };

        return update;
    }

    /// <summary>
    /// Extracts the role from a ChatChoice (handling both message and delta)
    /// </summary>
    private static ChatRole ExtractRoleFromChoice(ChatChoice? choice)
    {
        var roleString = choice?.Message?.Role ?? choice?.Delta?.Role;

        return roleString?.ToLowerInvariant() switch
        {
            "assistant" => ChatRole.Assistant,
            "user" => ChatRole.User,
            "system" => ChatRole.System,
            "tool" => ChatRole.Tool,
            _ => ChatRole.Assistant
        };
    }

    /// <summary>
    /// Converts a GitHub Copilot ChatChoice to Microsoft.Extensions.AI ChatMessage
    /// </summary>
    private static Microsoft.Extensions.AI.ChatMessage ToAiChatMessage(ChatChoice choice)
    {
        var contents = new List<AIContent>();

        // Add text content if available
        if (choice.Message?.Content?.AsText() is { } text && !string.IsNullOrEmpty(text))
        {
            contents.Add(new AITextContent(text));
        }

        // Add tool calls if available
        if (choice.Message?.ToolCalls != null)
        {
            foreach (var toolCall in choice.Message.ToolCalls)
            {
                var arguments = ParseArguments(toolCall.Function.Arguments);
                contents.Add(new FunctionCallContent(
                    toolCall.Id,
                    toolCall.Function.Name,
                    arguments
                ));
            }
        }

        var role = choice.Message?.Role.ToLowerInvariant() switch
        {
            "assistant" => ChatRole.Assistant,
            "user" => ChatRole.User,
            "system" => ChatRole.System,
            "tool" => ChatRole.Tool,
            _ => ChatRole.Assistant
        };

        return new Microsoft.Extensions.AI.ChatMessage(role, contents)
        {
            AuthorName = choice.Message?.Name,
            RawRepresentation = choice
        };
    }

    /// <summary>
    /// Converts GitHub Copilot finish reason to Microsoft.Extensions.AI ChatFinishReason
    /// </summary>
    private static ChatFinishReason? ToAiFinishReason(string? finishReason)
    {
        return finishReason?.ToLowerInvariant() switch
        {
            "stop" => ChatFinishReason.Stop,
            "length" => ChatFinishReason.Length,
            "tool_calls" => ChatFinishReason.ToolCalls,
            "content_filter" => ChatFinishReason.ContentFilter,
            null => null,
            _ => new ChatFinishReason(finishReason)
        };
    }

    /// <summary>
    /// Parses JSON arguments string to a dictionary
    /// </summary>
    private static IDictionary<string, object?> ParseArguments(string? argumentsJson)
    {
        if (string.IsNullOrWhiteSpace(argumentsJson))
        {
            return new Dictionary<string, object?>();
        }

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object?>>(argumentsJson)
                   ?? new Dictionary<string, object?>();
        }
        catch
        {
            return new Dictionary<string, object?>();
        }
    }

    /// <summary>
    /// Converts Microsoft.Extensions.AI chat messages to GitHub Copilot chat messages
    /// </summary>
    private static IEnumerable<ChatMessage> ToCopilotChatMessages(IEnumerable<Microsoft.Extensions.AI.ChatMessage> messages, ChatOptions? options)
    {
        // Add system instructions if provided in options
        if (!string.IsNullOrWhiteSpace(options?.Instructions))
        {
            yield return new ChatMessage(
                Role: "system",
                Content: MessageContent.FromText(options.Instructions)
            );
        }

        foreach (var message in messages)
        {
            yield return ToCopilotChatMessage(message);
        }
    }

    /// <summary>
    /// Converts a single Microsoft.Extensions.AI ChatMessage to GitHub Copilot ChatMessage
    /// </summary>
    private static ChatMessage ToCopilotChatMessage(Microsoft.Extensions.AI.ChatMessage message)
    {
        var role = message.Role.Value.ToLowerInvariant();
        var content = ExtractMessageContent(message.Contents);
        var toolCalls = ExtractToolCalls(message.Contents);
        var toolCallId = ExtractToolCallId(message.Contents);

        return new ChatMessage(
            Role: role,
            Content: content,
            Name: message.AuthorName,
            ToolCalls: toolCalls,
            ToolCallId: toolCallId
        );
    }

    /// <summary>
    /// Extracts message content from AI contents
    /// </summary>
    private static MessageContent? ExtractMessageContent(IList<AIContent> contents)
    {
        var parts = new List<ContentPart>();
        var textBuffer = new List<string>();
        bool hasNonText = false;

        foreach (var content in contents)
        {
            switch (content)
            {
                case FunctionCallContent:
                    // handled in tools
                    break;

                case AITextContent textContent:
                    if(!string.IsNullOrWhiteSpace(textContent.Text))
                    {
                        textBuffer.Add(textContent.Text);
                    }

                    break;
                case FunctionResultContent resultContent:
                    // For tool results, serialize the result
                    var resultText = resultContent.Result switch
                    {
                        string str => str,
                        null => string.Empty,
                        _ => JsonSerializer.Serialize(resultContent.Result)
                    };

                    if(!string.IsNullOrWhiteSpace(resultText))
                    {
                        textBuffer.Add(resultText);
                    }
                    break;
                case TextReasoningContent reasoningContent:
                    textBuffer.Add("[Reasoning] " + reasoningContent.Text);
                    break;
                case ErrorContent:
                    //textBuffer.Add($"[Error: {errorContent.Message}{(string.IsNullOrEmpty(errorContent.ErrorCode) ? "" : $" (Code: {errorContent.ErrorCode})")}{(string.IsNullOrEmpty(errorContent.Details) ? "" : $" - {errorContent.Details}") }]");
                    break;
                // throw new NotSupportedException($"FunctionCallContent is not supported for chat message content yet. Please implement handling if needed.");
                // TODO: The following types could not be resolved. Ensure your project references the correct Microsoft.Extensions.AI.Abstractions assembly.
                // case HostedFileContent hostedFileContent:
                //     hasNonText = true;
                //     parts.Add(new AI.GithubCopilot.Infrastructure.Models.TextPart($"[HostedFile: {hostedFileContent.FileId}]") );
                //     break;
                // case HostedVectorStoreContent hostedVectorStoreContent:
                //     hasNonText = true;
                //     parts.Add(new AI.GithubCopilot.Infrastructure.Models.TextPart($"[HostedVectorStore: {hostedVectorStoreContent.VectorStoreId}]") );
                //     break;
                case UriContent uriContent:
                    hasNonText = true;
                    if (uriContent.MediaType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                    {
                        parts.Add(new ImagePart(new ImageUrl(uriContent.Uri.ToString())));
                    }
                    else
                    {
                        throw new NotSupportedException($"UriContent with media type '{uriContent.MediaType}' is not supported as chat content.");
                    }
                    break;
                case DataContent dataContent:
                    hasNonText = true;
                    if (dataContent.MediaType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                    {
                        parts.Add(new ImagePart(new ImageUrl(dataContent.Uri)));
                    }
                    else
                    {
                        throw new NotSupportedException($"DataContent with media type '{dataContent.MediaType}' is not supported as chat content.");
                    }
                    break;
                case UsageContent:
                    //textBuffer.Add($"[Usage: {System.Text.Json.JsonSerializer.Serialize(usageContent.Details)}]");
                    break;
                default:
                    throw new NotSupportedException($"AIContent type '{content.GetType().Name}' is not supported in ExtractMessageContent.");
            }
        }

        // If only textBuffer is filled, return TextContent or null
        if (!hasNonText && parts.Count == 0)
        {
            if (textBuffer.Count == 0)
                return null;
            if (textBuffer.Count == 1)
                return MessageContent.FromText(textBuffer[0]);
            // Multiple text parts: return MultipartContent
            return MessageContent.FromText(string.Join("\n", textBuffer));
        }

        // If there are non-text parts, combine textBuffer as TextParts and add to parts
        foreach (var t in textBuffer)
            parts.Add(new TextPart(t));

        return parts.Count > 0
            ? MessageContent.Multipart(parts.ToArray())
            : null;
    }

    /// <summary>
    /// Extracts tool calls from AI contents
    /// </summary>
    private static IReadOnlyList<ToolCall>? ExtractToolCalls(IList<AIContent> contents)
    {
        var toolCalls = new List<ToolCall>();

        foreach (var content in contents)
        {
            if (content is FunctionCallContent functionCall)
            {
                var argumentsJson = functionCall.Arguments switch
                {
                    { } dict => JsonSerializer.Serialize(dict),
                    _ => JsonSerializer.Serialize(functionCall.Arguments)
                };

                toolCalls.Add(new ToolCall(
                    Id: functionCall.CallId,
                    Type: "function",
                    Function: new FunctionCall(
                        Name: functionCall.Name,
                        Arguments: argumentsJson
                    )
                ));
            }
        }

        return toolCalls.Count > 0 ? toolCalls : null;
    }

    /// <summary>
    /// Extracts tool call ID for tool result messages
    /// </summary>
    private static string? ExtractToolCallId(IList<AIContent> contents)
    {
        return contents.OfType<FunctionResultContent>().FirstOrDefault()?.CallId;
    }

    /// <summary>
    /// Converts Microsoft.Extensions.AI tools to GitHub Copilot tools
    /// </summary>
    private static IReadOnlyList<Tool>? ToCopilotTools(IEnumerable<AITool>? tools)
    {
        if (tools == null)
        {
            return null;
        }

        var copilotTools = new List<Tool>();

        foreach (var tool in tools)
        {
            if (tool is AIFunction function)
            {
                copilotTools.Add(new Tool(
                    Type: "function",
                    Function: new FunctionDefinition(
                        Name: function.Name,
                        Description: function.Description ?? string.Empty,
                        Parameters: function.JsonSchema
                    )
                ));
            }
        }

        return copilotTools.Count > 0 ? copilotTools : null;
    }
    /// <summary>
    /// Converts Microsoft.Extensions.AI tool mode to GitHub Copilot tool choice
    /// </summary>
    private static ToolChoice? ToCopilotToolChoice(ChatToolMode? toolMode)
    {
        return toolMode switch
        {
            AutoChatToolMode => ToolChoice.Auto,
            RequiredChatToolMode { RequiredFunctionName: not null } required =>
                ToolChoice.ForFunction(required.RequiredFunctionName),
            RequiredChatToolMode => ToolChoice.Required,
            _ => null
        };
    }

    /// <summary>
    /// Converts Microsoft.Extensions.AI response format to GitHub Copilot response format
    /// </summary>
    private static ResponseFormat? ToCopilotResponseFormat(ChatResponseFormat? responseFormat)
    {
        return responseFormat switch
        {
            ChatResponseFormatText => new ResponseFormat("text"),
            ChatResponseFormatJson jsonFormat => new ResponseFormat(
                "json_object",
                jsonFormat.Schema.HasValue ? new JsonSchema(
                    "object",
                    jsonFormat.SchemaDescription ?? "Generated schema",
                    jsonFormat.Schema.Value
                ) : null
            ),
            _ => null
        };
    }

    /// <summary>
    /// Converts an enumerable of GitHub Copilot ChatCompletionResponse to an async enumerable of ChatResponseUpdate for streaming
    /// </summary>
    public static async IAsyncEnumerable<ChatResponseUpdate> ToChatResponseUpdateStream(
        this IAsyncEnumerable<ChatCompletionResponse?> responseStream,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        string? responseId = null;

        await foreach (var response in responseStream.WithCancellation(cancellationToken))
        {
            if (response != null)
            {
                // Use the first response ID for all updates in the stream
                responseId ??= response.Id;

                yield return response.ToChatResponseUpdate(responseId);
            }
        }
    }
}
