using System.Security.Claims;

namespace MCP.Application.Interfaces;

/// <summary>
///     Service for JWT token operations
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    ///     Generates a JWT token for the specified user
    /// </summary>
    /// <param name="userName">The username</param>
    /// <param name="roles">The user roles</param>
    /// <param name="expiryMinutes">Token expiry in minutes (default: 60)</param>
    /// <returns>The JWT token</returns>
    string GenerateToken(string userName, string[] roles, int expiryMinutes = 60);

    /// <summary>
    ///     Validates and parses a JWT token
    /// </summary>
    /// <param name="token">The JWT token</param>
    /// <returns>The claims principal if valid, null otherwise</returns>
    ClaimsPrincipal? ValidateToken(string token);
}
