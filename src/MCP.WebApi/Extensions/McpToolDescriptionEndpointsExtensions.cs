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
                var response = await mcpClientToolProviderService.DescribeAsync(cancellationToken);
                return Results.Ok(response);
            })
            .WithName("GetMcpToolDescriptions")
            .WithSummary("Get description of MCP tools")
            .WithDescription("Return all available MCP tools descriptions")
            .WithTags("MCP Tools")
            .Produces<IDictionary<string, IList<McpToolDescription>>>()
            .WithOpenApi();

        return app;
    }
}

