namespace MCP.WebApi.Extensions;

public static class AsyncEnumerableExtensions
{

    private const string StreamingContentType = "application/json-stream";
    public static IResult ToResult<T>(this IAsyncEnumerable<T> responseStream, CancellationToken cancellationToken)
    {
        return Results.Stream(
            async (stream) =>
            {
                await foreach (var update in responseStream.WithCancellation(cancellationToken))
                {
                    var data = System.Text.Json.JsonSerializer.Serialize(update);
                    var bytes = System.Text.Encoding.UTF8.GetBytes(data +  Environment.NewLine);
                    await stream.WriteAsync(bytes, cancellationToken);
                    await stream.FlushAsync(cancellationToken);
                }
            },
            contentType: StreamingContentType
        );
    }

}
