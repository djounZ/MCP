using System.Text.Json;
using MCP.Tools.Infrastructure.Models;
using MCP.Tools.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace MCP.Tools.Infrastructure.Services;

public class McpServerConfigurationProviderService(IOptions<McpToolsOptions> options)
{
    private McpToolsOptions Options => options.Value;
    private readonly ReaderWriterLockSlim _lock = new();
    private McpServerConfiguration? _mcpServerConfiguration;

    private McpServerConfiguration? GetMcpServerConfigurationSafe()
    {

        if(_lock.TryEnterReadLock(-1))
        {
            try
            {
                return _mcpServerConfiguration;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
        return null;
    }

    private McpServerConfiguration? SetMcpServerConfiguration(Func<McpServerConfiguration?> func)
    {
        if(_lock.TryEnterWriteLock(-1))
        {
            try
            {
                return  SetMcpServerConfigurationUnsafe(func);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
        return null;
    }

    private McpServerConfiguration? SetMcpServerConfigurationUnsafe(Func<McpServerConfiguration?> func)
    {
        var mcpServerConfiguration = func();
        if (mcpServerConfiguration == null)
        {
            using var stream = File.OpenRead(Options.McpServerConfigurationFilePath);
            _mcpServerConfiguration =  JsonSerializer.Deserialize<McpServerConfiguration>(stream)
                   ?? throw new InvalidOperationException("Failed to deserialize MCP server configuration.");

            return _mcpServerConfiguration;
        }

        using var fileStream = new FileStream(Options.McpServerConfigurationFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
        JsonSerializer.Serialize(fileStream, mcpServerConfiguration);
        _mcpServerConfiguration = mcpServerConfiguration;
        return _mcpServerConfiguration;
    }

    public McpServerConfiguration GetMcpServerConfiguration()
    {
        var mcpServerConfiguration = GetMcpServerConfigurationSafe();
        if (mcpServerConfiguration != null)
        {
            return mcpServerConfiguration;
        }

        var mcpServerConfigurationAsync =  SetMcpServerConfiguration(()=> null) ?? throw new InvalidOperationException("MCP server configuration is not set.");
        return mcpServerConfigurationAsync;
    }

    // CRUD methods for Servers property
    public IDictionary<string, McpServerConfigurationItem> GetAllServers()
    {
        return GetMcpServerConfiguration().Servers;
    }

    public McpServerConfigurationItem? GetServer(string serverName)
    {
        return GetMcpServerConfiguration().Servers.TryGetValue(serverName, out var item) ? item : null;
    }

    public bool CreateServer(string serverName, McpServerConfigurationItem item)
    {
        var config =  GetMcpServerConfiguration();
        if (!config.Servers.TryAdd(serverName, item))
        {
            return false;
        }

        SetMcpServerConfiguration(()=>config);
        return true;
    }

    public bool UpdateServer(string serverName, McpServerConfigurationItem item)
    {
        var config =  GetMcpServerConfiguration();
        if (!config.Servers.ContainsKey(serverName))
        {
            return false;
        }

        config.Servers[serverName] = item;
         SetMcpServerConfiguration(()=>config);
        return true;
    }

    public bool DeleteServer(string serverName)
    {
        var config =  GetMcpServerConfiguration();
        if (!config.Servers.Remove(serverName))
        {
            return false;
        }

        var mcpServerConfigurationAsync =  SetMcpServerConfiguration(()=> config);
        return mcpServerConfigurationAsync != null;
    }
}
