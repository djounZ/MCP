using System.Text.Json.Serialization;

namespace AI.GithubCopilot.Infrastructure.Models;

public record GithubCopilotAccessTokenResponse(
    [property: JsonPropertyName("token")] string Token,
    [property: JsonPropertyName("expires_at")] long ExpiresAt
);