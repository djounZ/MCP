using System.Text.Json.Serialization;

namespace MCP.Application.DTOs.AI.Provider;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AiProviderEnum
{
    [JsonStringEnumMemberName("github_copilot")]
    GithubCopilot = 1,
    [JsonStringEnumMemberName("ollama")]
    Ollama = 2/*,
    OpenAI = 3,
    Anthropic = 4*/
}
