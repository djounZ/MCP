namespace AI.GithubCopilot.Infrastructure.Models;

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