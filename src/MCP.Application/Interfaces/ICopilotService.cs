using MCP.Application.Models.Copilot;
using MCP.Domain.Common;

namespace MCP.Application.Interfaces;

/// <summary>
///     Interface for GitHub Copilot service
/// </summary>
public interface ICopilotService
{
    /// <summary>
    ///     Get code completion from Copilot
    /// </summary>
    /// <param name="prompt">The code prompt</param>
    /// <param name="language">Programming language (default: python)</param>
    /// <returns>Generated code completion or error</returns>
    Task<Result<string>> GetCompletionAsync(string prompt, string language = "python");

    Task<Result<GithubDeviceCodeResponse>> RegisterDevice();

    bool IsDeviceRegistered { get; }
}
