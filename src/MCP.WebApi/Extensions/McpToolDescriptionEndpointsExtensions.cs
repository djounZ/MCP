using MCP.Tools.Infrastructure.Models;
using MCP.Tools.Infrastructure.Services;

namespace MCP.WebApi.Extensions;

public static class McpToolDescriptionEndpointsExtensions
{
      public static WebApplication MapMcpToolsEndpoints(this WebApplication app)
    {
        // provider
        app.MapGet("/api/mcp_tools/descriptions", async (
                McpClientToolProviderService mcpClientToolProviderService,
                CancellationToken cancellationToken) =>
            {
                var response = await mcpClientToolProviderService.GetAll(cancellationToken);
                return Results.Ok(response);
            })
            .WithName("GetMcpToolDescriptions")
            .WithSummary("Get description of MCP tools")
            .WithDescription("Return all available MCP tools descriptions")
            .WithTags("MCP Tools")
            .Produces<IDictionary<string, IList<McpClientToolDescription>>>()
            .WithOpenApi();

        app.MapPost("/api/mcp_tools/call", async (
                McpClientToolProviderService mcpClientToolProviderService,
                McpClientToolRequest request,
                CancellationToken cancellationToken) =>
            {
                var response = await mcpClientToolProviderService.CallToolAsync(request, cancellationToken);
                return Results.Ok(response);
            })
            .WithName("CallMcpTool")
            .WithSummary("Call an MCP tool")
            .WithDescription("Invoke a tool on a configured MCP server and return the result.")
            .WithTags("MCP Tools")
            .Accepts<McpClientToolRequest>("application/json")
            .Produces<McpClientToolResponse>()
            .WithOpenApi();

        return app;
    }
}

