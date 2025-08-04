// ReSharper disable PropertyCanBeMadeInitOnly.Global
namespace MCP.Infrastructure.Options;

public class OllamaOptions
{
    public string Uri { get; set; } = string.Empty;
    public string DefaultModel { get; set; } = string.Empty;
}
