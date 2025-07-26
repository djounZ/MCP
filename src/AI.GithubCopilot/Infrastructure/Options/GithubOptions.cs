namespace AI.GithubCopilot.Infrastructure.Options;

public class GithubOptions
{
    public string ClientId { get; set; } = string.Empty;
    public string DeviceCodeUrl { get; set; } = string.Empty;

    public Dictionary<string, string> Headers { get; set; } = new();
    public string Scope { get; set; } = string.Empty;
    public string GrantType { get; set; } =string.Empty;

    public string TokenUrl { get; set; } = string.Empty;
}
