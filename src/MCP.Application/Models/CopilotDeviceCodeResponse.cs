namespace MCP.Application.Models;

/// <summary>
/// Represents the response from a device code registration request for Copilot authentication
/// </summary>
public class CopilotDeviceCodeResponse
{
    /// <summary>
    /// The device verification code
    /// </summary>
    public string DeviceCode { get; init; } = string.Empty;
    
    /// <summary>
    /// The user code that should be entered on the verification page
    /// </summary>
    public string UserCode { get; init; } = string.Empty;
    
    /// <summary>
    /// The URI where the user should go to enter the user code
    /// </summary>
    public string VerificationUri { get; init; } = string.Empty;
    
    /// <summary>
    /// The lifetime in seconds of the device code and user code
    /// </summary>
    public int ExpiresIn { get; init; }
    
    /// <summary>
    /// The minimum number of seconds that must pass before you can make a new access token request
    /// </summary>
    public int Interval { get; init; }
}
