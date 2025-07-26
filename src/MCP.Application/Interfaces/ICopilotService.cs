using MCP.Application.Models;
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
    public Task<Result<string>> GetCompletionAsync(string prompt, string language = "python");

    public Task<Result<CopilotDeviceCodeResponse>> RegisterDeviceAsync();
    public Task<Result<Unit>> GetAccessTokenAsync();

    public bool HasAccessToken { get; }
}
