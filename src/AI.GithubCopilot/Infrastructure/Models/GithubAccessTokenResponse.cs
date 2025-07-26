using System.Text.Json.Serialization;

namespace AI.GithubCopilot.Infrastructure.Models;

public record GithubAccessTokenResponse
{
    [JsonPropertyName("access_token")] public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("token_type")] public string TokenType { get; set; } = string.Empty;

    [JsonPropertyName("scope")] public string Scope { get; set; } = string.Empty;
}

public record GithubAccessTokenRequest
{
    [JsonPropertyName("client_id")] public string ClientId { get; init; } = string.Empty;

    [JsonPropertyName("device_code")] public string DeviceCode { get; init; } = string.Empty;

    [JsonPropertyName("grant_type")] public string GrantType { get; init; } = string.Empty;
}
