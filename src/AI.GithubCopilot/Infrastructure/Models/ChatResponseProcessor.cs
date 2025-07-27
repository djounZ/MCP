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

        // Note: CopilotReferences were removed from the updated model
        // If needed, they should be handled separately or added back

        // Get the first choice or fall back to the response itself
        ChatChoice? choice = response.Choices?.FirstOrDefault();

        // Extract content from message or delta
        string? content = choice?.Message?.Content?.AsText() ?? choice?.Delta?.Content;

        // Get usage information
        int? totalTokens = response.Usage?.TotalTokens;

        // Get finish reason from the first choice
        string? finishReason = choice?.FinishReason;

        return new ProcessedChatResponse(
            Content: content,
            FinishReason: finishReason,
            TotalTokens: totalTokens,
            References: references.AsReadOnly()
        );
    }
}