using AI.GithubCopilot.Infrastructure.Models;

namespace AI.GithubCopilot.Infrastructure.Extensions;

/// <summary>
/// Extension methods for ChatCompletionResponse
/// </summary>
public static class ChatCompletionResponseExtensions
{
    /// <summary>
    /// Gets the first choice's content, if available
    /// </summary>
    public static string? GetContent(this ChatCompletionResponse response)
    {
        var choice = response.Choices.FirstOrDefault();
        return choice?.Message?.Content?.AsText() ?? choice?.Delta?.Content;
    }
    
    /// <summary>
    /// Gets all tool calls from the first choice
    /// </summary>
    public static IReadOnlyList<ToolCall> GetToolCalls(this ChatCompletionResponse response)
    {
        return response.Choices.FirstOrDefault()?.Message?.ToolCalls ?? Array.Empty<ToolCall>();
    }
    
    /// <summary>
    /// Indicates if the response is complete (not streaming)
    /// </summary>
    public static bool IsComplete(this ChatCompletionResponse response)
    {
        return response.Choices.FirstOrDefault()?.FinishReason != null;
    }
    
    /// <summary>
    /// Indicates if the response was stopped due to content filtering
    /// </summary>
    public static bool WasFiltered(this ChatCompletionResponse response)
    {
        var choice = response.Choices.FirstOrDefault();
        return choice?.FinishReason == "content_filter" || 
               choice?.ContentFilterResults?.IsFiltered() == true;
    }
    
    /// <summary>
    /// Gets the finish reason for the first choice
    /// </summary>
    public static string? GetFinishReason(this ChatCompletionResponse response)
    {
        return response.Choices.FirstOrDefault()?.FinishReason;
    }
    
    /// <summary>
    /// Gets the refusal message if the model refused to respond
    /// </summary>
    public static string? GetRefusal(this ChatCompletionResponse response)
    {
        var choice = response.Choices.FirstOrDefault();
        return choice?.Message?.Refusal ?? choice?.Delta?.Refusal;
    }
    
    /// <summary>
    /// Indicates if any content was filtered
    /// </summary>
    public static bool IsFiltered(this ContentFilterResults? results)
    {
        return results?.Hate.Filtered == true ||
               results?.SelfHarm.Filtered == true ||
               results?.Sexual.Filtered == true ||
               results?.Violence.Filtered == true;
    }
    
    /// <summary>
    /// Gets the highest severity level from content filter results
    /// </summary>
    public static string? GetHighestFilterSeverity(this ContentFilterResults? results)
    {
        if (results == null) return null;
        
        var severities = new[] { results.Hate.Severity, results.SelfHarm.Severity, results.Sexual.Severity, results.Violence.Severity }
            .Where(s => !string.IsNullOrEmpty(s))
            .ToList();
            
        if (severities.Contains("high")) return "high";
        if (severities.Contains("medium")) return "medium";
        if (severities.Contains("low")) return "low";
        if (severities.Contains("safe")) return "safe";
        
        return null;
    }
}
