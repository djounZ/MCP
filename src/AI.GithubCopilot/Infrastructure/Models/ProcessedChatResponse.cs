namespace AI.GithubCopilot.Infrastructure.Models;

/// <summary>
/// Processed output that matches the Lua prepare_output function
/// </summary>
public record ProcessedChatResponse(
    string? Content,
    string? FinishReason,
    int? TotalTokens,
    IReadOnlyList<CopilotReference> References
);
