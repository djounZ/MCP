using MCP.Tools.Infrastructure.Mappers;
using MCP.Tools.Infrastructure.Models;
using MCP.Tools.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Client;

namespace MCP.Tools.Infrastructure.Services;

public class McpClientToolProvider(IOptions<McpToolsOptions> options, ClientTransportFactoryService clientTransportFactoryService, McpServerConfigurationMapper mcpServerConfigurationMapper)
{

    private McpToolsOptions Options => options.Value;

    public async Task<IDictionary<string, IList<McpToolDescription>>> DescribeAsync()
    {
        var mcpToolDescriptions = new Dictionary<string, IList<McpToolDescription>>();

        var clientTransports = clientTransportFactoryService.Create(Options.McpServerConfiguration);

        foreach (var (name, clientTransport) in clientTransports)
        {
            await using var client = await McpClientFactory.CreateAsync(clientTransport);
            var mcpClientTools = await client.ListToolsAsync();
            mcpToolDescriptions[name] = mcpServerConfigurationMapper.Map(mcpClientTools);
        }
        return mcpToolDescriptions;
    }
}
