// ReSharper disable PropertyCanBeMadeInitOnly.Global
namespace AI.GithubCopilot.Infrastructure.Options;

public class GithubOptions
{
    public string ClientId { get; set; } = string.Empty;
    public string DeviceCodeUrl { get; set; } = string.Empty;
    public Dictionary<string, string> DeviceCodeHeaders { get; set; } = [];


    public string TokenUrl { get; set; } = string.Empty;
    public Dictionary<string, string> TokenHeaders { get; set; } = [];
    public string DeviceScope { get; set; } = string.Empty;
    public string GrantType { get; set; } =string.Empty;
    public string CopilotTokenUrl { get; set; } = string.Empty;
    public Dictionary<string, string> CopilotTokenHeaders { get; set; } = [];

    public string CopilotChatCompletionsUrl { get; set; } = string.Empty;
    public Dictionary<string, string> CopilotChatCompletionsHeaders { get; set; } = [];

    public string CopilotModelsUrl { get; set; } = string.Empty;
}
