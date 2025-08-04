using System.Text.Json.Serialization;

namespace MCP.Application.DTOs.AI.ChatCompletion;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ChatClientProviderEnum
{
    [JsonStringEnumMemberName("github_copilot")]
    GithubCopilot = 1,
    [JsonStringEnumMemberName("ollama")]
    Ollama = 2/*,
    OpenAI = 3,
    Anthropic = 4*/
}
