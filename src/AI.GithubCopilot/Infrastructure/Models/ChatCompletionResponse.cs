using System.Text.Json.Serialization;

namespace AI.GithubCopilot.Infrastructure.Models;

/// <summary>
/// Represents a reference returned by Copilot
/// </summary>
public record CopilotReference(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("url")] string Url
);

/// <summary>
/// Metadata for copilot references
/// </summary>
public record CopilotReferenceMetadata(
    [property: JsonPropertyName("display_name")] string? DisplayName,
    [property: JsonPropertyName("display_url")] string? DisplayUrl
);

/// <summary>
/// Full copilot reference with metadata
/// </summary>
public record CopilotReferenceWithMetadata(
    [property: JsonPropertyName("metadata")] CopilotReferenceMetadata? Metadata
);

/// <summary>
/// Usage statistics for the completion
/// </summary>
public record TokenUsage(
    [property: JsonPropertyName("total_tokens")] int? TotalTokens,
    [property: JsonPropertyName("prompt_tokens")] int? PromptTokens,
    [property: JsonPropertyName("completion_tokens")] int? CompletionTokens
);

/// <summary>
/// Delta content for streaming responses
/// </summary>
public record MessageDelta(
    [property: JsonPropertyName("content")] string? Content,
    [property: JsonPropertyName("role")] string? Role
);

/// <summary>
/// Complete message for non-streaming responses
/// </summary>
public record ResponseMessage(
    [property: JsonPropertyName("content")] string? Content,
    [property: JsonPropertyName("role")] string? Role
);

/// <summary>
/// Individual choice in the completion response
/// </summary>
public record CompletionChoice(
    [property: JsonPropertyName("message")] ResponseMessage? Message,
    [property: JsonPropertyName("delta")] MessageDelta? Delta,
    [property: JsonPropertyName("finish_reason")] string? FinishReason,
    [property: JsonPropertyName("done_reason")] string? DoneReason,
    [property: JsonPropertyName("usage")] TokenUsage? Usage
);

/// <summary>
/// Main response from GitHub Copilot Chat Completions API
/// </summary>
public record ChatCompletionResponse(
    [property: JsonPropertyName("choices")] IReadOnlyList<CompletionChoice>? Choices,
    [property: JsonPropertyName("usage")] TokenUsage? Usage,
    [property: JsonPropertyName("copilot_references")] IReadOnlyList<CopilotReferenceWithMetadata>? CopilotReferences,
    [property: JsonPropertyName("finish_reason")] string? FinishReason,
    [property: JsonPropertyName("done_reason")] string? DoneReason
);

/// <summary>
/// Processed output that matches the Lua prepare_output function
/// </summary>
public record ProcessedChatResponse(
    string? Content,
    string? FinishReason,
    int? TotalTokens,
    IReadOnlyList<CopilotReference> References
);

/// <summary>
/// Helper class to process raw API responses into structured format
/// </summary>
public static class ChatResponseProcessor
{
    /// <summary>
    /// Processes a raw ChatCompletionResponse into a simplified ProcessedChatResponse
    /// This mimics the prepare_output function from the Lua code
    /// </summary>
    public static ProcessedChatResponse ProcessResponse(ChatCompletionResponse response)
    {
        var references = new List<CopilotReference>();

        // Extract references from copilot_references
        if (response.CopilotReferences != null)
        {
            foreach (var reference in response.CopilotReferences)
            {
                var metadata = reference.Metadata;
                if (metadata?.DisplayName != null && metadata.DisplayUrl != null)
                {
                    references.Add(new CopilotReference(metadata.DisplayName, metadata.DisplayUrl));
                }
            }
        }

        // Get the first choice or fall back to the response itself
        CompletionChoice? message = null;
        if (response.Choices?.Count > 0)
        {
            message = response.Choices[0];
        }

        // Extract content from message or delta
        string? content = message?.Message?.Content ?? message?.Delta?.Content;

        // Get usage information
        int? totalTokens = message?.Usage?.TotalTokens ?? response.Usage?.TotalTokens;

        // Get finish reason
        string? finishReason = message?.FinishReason ?? 
                              message?.DoneReason ?? 
                              response.FinishReason ?? 
                              response.DoneReason;

        return new ProcessedChatResponse(
            Content: content,
            FinishReason: finishReason,
            TotalTokens: totalTokens,
            References: references.AsReadOnly()
        );
    }
}
