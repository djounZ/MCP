namespace MCP.Domain.Services.External;

/// <summary>
/// Interface for GitHub Copilot service
/// </summary>
public interface ICopilotService
{
    /// <summary>
    /// Setup authentication with GitHub Copilot
    /// </summary>
    Task SetupAsync();

    /// <summary>
    /// Get code completion from Copilot
    /// </summary>
    /// <param name="prompt">The code prompt</param>
    /// <param name="language">Programming language (default: python)</param>
    /// <returns>Generated code completion</returns>
    Task<string> GetCompletionAsync(string prompt, string language = "python");
}
