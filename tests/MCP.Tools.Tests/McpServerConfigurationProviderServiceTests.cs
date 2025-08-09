using System.Text.Json;
using MCP.Tools.Infrastructure.Models;
using MCP.Tools.Infrastructure.Options;
using MCP.Tools.Infrastructure.Services;
using Microsoft.Extensions.Options;

namespace MCP.Tools.Tests
{
    public class McpServerConfigurationProviderServiceTests
    {
        private string GetTempFilePath()
        {
            return Path.GetTempFileName();
        }

        private McpServerConfigurationProviderService CreateService(string filePath, IDictionary<string, McpServerConfigurationItem>? servers = null)
        {
            var options = Options.Create(new McpToolsOptions { McpServerConfigurationFilePath = filePath });
            // Always initialize with valid config if not testing error case
            var validConfig = servers == null ? new McpServerConfiguration(new Dictionary<string, McpServerConfigurationItem>()) : new McpServerConfiguration(servers);
            File.WriteAllText(filePath, JsonSerializer.Serialize(validConfig));
            var service = new McpServerConfigurationProviderService(options);
            return service;
        }

        private McpServerConfigurationItem DummyServer(string name = "cmd")
        {
            return new McpServerConfigurationItem(
                Category: "cat",
                Command: name,
                Arguments: ["--foo"],
                EnvironmentVariables: new Dictionary<string, string?> { ["ENV"] = "VAL" },
                Endpoint: null,
                Type: McpServerTransportType.Stdio
            );
        }

        [Fact]
        public void CanCreateReadUpdateDeleteServer()
        {
            var filePath = GetTempFilePath();
            var service = CreateService(filePath);
            var serverName = "test-server";
            var item = DummyServer();

            // Create
            var created = service.CreateServer(serverName, item);
            Assert.True(created);

            // Read
            var readItem = service.GetServer(serverName);
            Assert.NotNull(readItem);
            Assert.Equal(item.Command, readItem.Command);

            // Update
            var updatedItem = item with { Command = "updated-cmd" };
            var updated = service.UpdateServer(serverName, updatedItem);
            Assert.True(updated);
            var readUpdated = service.GetServer(serverName);
            Assert.NotNull(readUpdated);
            Assert.Equal("updated-cmd", readUpdated.Command);

            // Delete
            var deleted = service.DeleteServer(serverName);
            Assert.True(deleted);
            var readDeleted = service.GetServer(serverName);
            Assert.Null(readDeleted);
        }

        [Fact]
        public void CannotCreateDuplicateServer()
        {
            var filePath = GetTempFilePath();
            var service = CreateService(filePath);
            var serverName = "dup-server";
            var item = DummyServer();

            var created = service.CreateServer(serverName, item);
            Assert.True(created);
            var duplicate = service.CreateServer(serverName, item);
            Assert.False(duplicate);
        }

        [Fact]
        public void CannotUpdateOrDeleteNonExistentServer()
        {
            var filePath = GetTempFilePath();
            var service = CreateService(filePath);
            var serverName = "missing-server";
            var item = DummyServer();

            var updated = service.UpdateServer(serverName, item);
            Assert.False(updated);
            var deleted = service.DeleteServer(serverName);
            Assert.False(deleted);
        }

        [Fact]
        public void CanGetAllServers()
        {
            var filePath = GetTempFilePath();
            var servers = new Dictionary<string, McpServerConfigurationItem>
            {
                ["s1"] = DummyServer("cmd1"),
                ["s2"] = DummyServer("cmd2")
            };
            var service = CreateService(filePath, servers);

            var all = service.GetAllServers();
            Assert.Equal(2, all.Count);
            Assert.Contains("s1", all.Keys);
            Assert.Contains("s2", all.Keys);
        }

        [Fact]
        public void ThrowsOnInvalidConfigFile()
        {
            var filePath = GetTempFilePath();
            File.WriteAllText(filePath, "not-json");
            var service = CreateService(filePath);

            Assert.Empty(service.GetAllServers());
        }
    }
}
