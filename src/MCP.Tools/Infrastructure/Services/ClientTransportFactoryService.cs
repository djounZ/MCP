using MCP.Tools.Infrastructure.Mappers;
using MCP.Tools.Infrastructure.Models;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;

namespace MCP.Tools.Infrastructure.Services;

public class ClientTransportFactoryService(ILogger<ClientTransportFactoryService> logger, McpServerConfigurationMapper mapper,  ILoggerFactory? loggerFactory = null)
{


    public IDictionary<string, IClientTransport> Create(IDictionary<string, McpServerConfigurationItem> serverConfiguration)
    {
        var transports = new Dictionary<string, IClientTransport>();

        foreach (var kvp in serverConfiguration)
        {
            try
            {
                var transport = Create(kvp.Value);
                transports[kvp.Key] = transport;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to create transport for server {ServerName}", kvp.Key);
            }
        }

        return transports;
    }

    public IClientTransport Create(McpServerConfigurationItem configurationItem)
    {
        if (configurationItem.Type is McpServerTransportType.Stdio)
        {
            return new StdioClientTransport(mapper.MapStdioClientTransportOptions(configurationItem), loggerFactory);
        }

        if (configurationItem.Type is McpServerTransportType.Http)
        {
            return new SseClientTransport(mapper.MapSseClientTransport(configurationItem), loggerFactory);
        }

        logger.LogError("Unsupported configuration type: {ConfigurationType}", configurationItem.GetType().Name);
        throw new NotSupportedException($"Unsupported configuration type: {configurationItem.GetType().Name}");
    }

}
