using MCP.Tools.Infrastructure.Models;
using MCP.Tools.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace MCP.WebApi.Extensions;

public static class McpClientDescriptionEndpointsExtensions
{
    public static WebApplication MapMcpClientDescriptionEndpoints(this WebApplication app)
    {
        app.MapGet("/mcp-client-descriptions",
            async ([FromServices] McpClientDescriptionProviderService service, CancellationToken cancellationToken) =>
            {
                var descriptions = await service.GetAll(cancellationToken);
                return Results.Ok(descriptions);
            })
            .WithName("GetAllMcpClientDescriptions")
            .WithSummary("Get all MCP client descriptions")
            .WithDescription("Returns all MCP client descriptions for all configured servers")
            .WithTags("McpClientDescription")
            .Produces<IDictionary<string, McpClientDescription>>()
            .WithOpenApi();

        return app;
    }
}
