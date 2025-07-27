using System.Text.Json.Serialization;

namespace AI.GithubCopilot.Infrastructure.Models;

public class GithubDeviceCodeRequest
{
    [JsonPropertyName("client_id")]
    public string ClientId { get; init; } = string.Empty;

    [JsonPropertyName("scope")]
    public string Scope { get; init; } = string.Empty;
}