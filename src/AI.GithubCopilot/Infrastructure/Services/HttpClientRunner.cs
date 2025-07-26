using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using System.Text;
using System.Text.Json;

namespace AI.GithubCopilot.Infrastructure.Services;

public sealed class HttpClientRunner
{

    public async  Task<TOut> SendAsyncAndDeserialize<TIn,TOut>(
        TIn requestContent,
        HttpClient client,
        HttpMethod method,
        [StringSyntax(StringSyntaxAttribute.Uri)] string? requestUri,
        Dictionary<string, string> headers,
        HttpCompletionOption completionOption,
        JsonSerializerOptions? options,
        CancellationToken cancellationToken)
    {

        using var request = CreateHttpRequestMessage(method, requestUri, requestContent, headers, options);
        using var response = await client.SendAsync(request, completionOption, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await ReadContentAsync<TOut>(options, cancellationToken, response);
    }
    private static HttpRequestMessage CreateHttpRequestMessage<TIn>(HttpMethod method, string? requestUri,
        TIn requestContent, Dictionary<string, string> headers, JsonSerializerOptions? options)
    {
        HttpRequestMessage? request = null;
        try
        {
            request = new HttpRequestMessage(method, requestUri);

            foreach (var (name, value) in headers)
            {
                request.Headers.Add(name, value);
            }

            var json = JsonSerializer.Serialize(requestContent, options);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            return request;
        }
        catch
        {
            request?.Dispose();
            throw;
        }
    }

    private static async Task<TOut> ReadContentAsync<TOut>(JsonSerializerOptions? options,
        CancellationToken cancellationToken, HttpResponseMessage response)
    {
        await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);

        // Handle gzip decompression efficiently without buffering
        if (response.Content.Headers.ContentEncoding.Contains("gzip"))
        {
            await using var gzipStream = new GZipStream(responseStream, CompressionMode.Decompress);
            var result = await JsonSerializer.DeserializeAsync<TOut>(gzipStream, options, cancellationToken);
            return result!;
        }
        else
        {
            var result = await JsonSerializer.DeserializeAsync<TOut>(responseStream, options, cancellationToken);
            return result!;
        }
    }

}
