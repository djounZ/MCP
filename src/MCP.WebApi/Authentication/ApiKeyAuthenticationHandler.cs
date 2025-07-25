using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace MCP.WebApi.Extensions;

/// <summary>
///     Options for API Key authentication
/// </summary>
public class ApiKeyAuthenticationSchemeOptions : AuthenticationSchemeOptions
{
    public Dictionary<string, string> ApiKeys { get; set; } = new();
}

/// <summary>
///     Authentication handler for API Key authentication
/// </summary>
public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationSchemeOptions>
{
    private const string ApiKeyHeaderName = "X-API-Key";

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<ApiKeyAuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(ApiKeyHeaderName, out var apiKeyHeaderValues))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var providedApiKey = apiKeyHeaderValues.FirstOrDefault();

        if (string.IsNullOrWhiteSpace(providedApiKey))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        // Check if the API key exists and get the associated user
        if (Options.ApiKeys.TryGetValue(providedApiKey, out var userName))
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, userName), new Claim(ClaimTypes.NameIdentifier, userName),
                new Claim("apikey", providedApiKey)
            };

            // Add role claims based on username or API key configuration
            if (userName.Contains("admin", StringComparison.OrdinalIgnoreCase))
            {
                claims = claims.Append(new Claim("role", "admin")).ToArray();
            }
            else
            {
                claims = claims.Append(new Claim("role", "user")).ToArray();
            }

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            Logger.LogInformation("API Key authentication successful for user: {UserName}", userName);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }

        Logger.LogWarning("Invalid API Key provided: {ApiKey}",
            providedApiKey[..Math.Min(8, providedApiKey.Length)] + "...");
        return Task.FromResult(AuthenticateResult.Fail("Invalid API Key"));
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = 401;
        Response.Headers.Append("WWW-Authenticate", $"{Scheme.Name} realm=\"API\"");
        return Response.WriteAsync("API Key authentication required. Include X-API-Key header.");
    }
}
