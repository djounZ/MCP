namespace MCP.Infrastructure.Options;

public class CopilotServiceOptions
{
    public string ClientId { get; set; } = string.Empty;
    public string DeviceCodeUrl { get; set; } = string.Empty;
    public string AccessTokenUrl { get; set; } = string.Empty;
    public string TokenUrl { get; set; } = string.Empty;
    public string CompletionUrl { get; set; } = string.Empty;
}
