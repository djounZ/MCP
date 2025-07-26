namespace MCP.Infrastructure.Options;

public class CopilotServiceOptions
{
    public string ClientId { get; set; } = string.Empty;
    public string DeviceCodeUrl { get; set; } = string.Empty;
    public string GithubTokenUrl { get; set; } = string.Empty;
    public string GithubCopilotTokenUrl { get; set; } = string.Empty;
    public string CompletionUrl { get; set; } = string.Empty;

    // Technical headers
    public string EditorVersion { get; set; } = string.Empty;
    public string EditorPluginVersion { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public string AcceptEncoding { get; set; } = string.Empty;
}
