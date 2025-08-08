using MCP.Tools.Infrastructure.Models;

namespace MCP.Tools.Infrastructure.Options;

public class McpToolsOptions
{
    public required McpServerConfiguration McpServerConfiguration { get; set; }
}
