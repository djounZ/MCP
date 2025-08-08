using MCP.Tools.Infrastructure.Mappers;
using MCP.Tools.Infrastructure.Models;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;

namespace MCP.Tools.Infrastructure.Services;

public class ClientTransportFactoryService(ILogger<ClientTransportFactoryService> logger, McpServerConfigurationMapper mapper,  ILoggerFactory? loggerFactory = null)
{


    public IDictionary<string, IClientTransport> Create(McpServerConfiguration configuration)
    {
        var transports = new Dictionary<string, IClientTransport>();

        foreach (var kvp in configuration.Servers)
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

    private IClientTransport Create(McpServerConfigurationItem configurationItem)
    {
        if (configurationItem is McpServerConfigurationItemStdio stdioConfig)
        {
            return new StdioClientTransport(mapper.Map(stdioConfig), loggerFactory);
        }

        if (configurationItem is McpServerConfigurationItemHttp httpConfig)
        {
            return new SseClientTransport(mapper.Map(httpConfig), loggerFactory);
        }

        logger.LogError("Unsupported configuration type: {ConfigurationType}", configurationItem.GetType().Name);
        throw new NotSupportedException($"Unsupported configuration type: {configurationItem.GetType().Name}");
    }

}
