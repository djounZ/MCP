using MCP.Tools.Infrastructure.Models;
using MCP.Tools.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace MCP.WebApi.Extensions;

public static class McpServerConfigurationEndpointsExtensions
{
    public static WebApplication MapMcpServerConfigurationEndpoints(this WebApplication app)
    {
        app.MapGet("/mcp-servers",
                ([FromServices] McpServerConfigurationProviderService service) => Results.Ok((object?)service.GetAllServers()))
            .WithName("GetAllMcpServers")
            .WithSummary("Get all MCP servers")
            .WithDescription("Returns all MCP server configuration items")
            .WithTags("McpServerConfiguration")
            .Produces<IDictionary<string, McpServerConfigurationItem>>()
            .WithOpenApi();

        app.MapGet("/mcp-servers/{serverName}",
                ([FromServices] McpServerConfigurationProviderService service, string serverName) =>
                {
                    var item = service.GetServer(serverName);
                    return item is not null ? Results.Ok(item) : Results.NotFound();
                })
            .WithName("GetMcpServerByName")
            .WithSummary("Get MCP server by name")
            .WithDescription("Returns a single MCP server configuration item by name")
            .WithTags("McpServerConfiguration")
            .Produces<McpServerConfigurationItem>()
            .Produces(StatusCodes.Status404NotFound)
            .WithOpenApi();

        app.MapPost("/mcp-servers/{serverName}", ([FromServices] McpServerConfigurationProviderService service,
                string serverName, [FromBody] McpServerConfigurationItem item) =>
            {
                var created = service.CreateServer(serverName, item);
                return created
                    ? Results.Created($"/mcp-servers/{serverName}", item)
                    : Results.Conflict($"Server '{serverName}' already exists.");
            })
            .WithName("CreateMcpServer")
            .WithSummary("Create MCP server")
            .WithDescription("Creates a new MCP server configuration item")
            .WithTags("McpServerConfiguration")
            .Produces<McpServerConfigurationItem>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status409Conflict)
            .WithOpenApi();

        app.MapPut("/mcp-servers/{serverName}", ([FromServices] McpServerConfigurationProviderService service,
                string serverName, [FromBody] McpServerConfigurationItem item) =>
            {
                var updated = service.UpdateServer(serverName, item);
                return updated ? Results.Ok(item) : Results.NotFound($"Server '{serverName}' not found.");
            })
            .WithName("UpdateMcpServer")
            .WithSummary("Update MCP server")
            .WithDescription("Updates an existing MCP server configuration item")
            .WithTags("McpServerConfiguration")
            .Produces<McpServerConfigurationItem>()
            .Produces(StatusCodes.Status404NotFound)
            .WithOpenApi();

        app.MapDelete("/mcp-servers/{serverName}",
                ([FromServices] McpServerConfigurationProviderService service, string serverName) =>
                {
                    var deleted = service.DeleteServer(serverName);
                    return deleted ? Results.NoContent() : Results.NotFound($"Server '{serverName}' not found.");
                })
            .WithName("DeleteMcpServer")
            .WithSummary("Delete MCP server")
            .WithDescription("Deletes an MCP server configuration item")
            .WithTags("McpServerConfiguration")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithOpenApi();

        return app;
    }
}
