using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace AI.GithubCopilot.Infrastructure.Services;

public sealed class HttpClientRunner(ILogger<HttpClientRunner> logger)
{


    public async Task<string?> SendAndReadAsStringAsync(
        HttpClient client,
        HttpMethod method,
        [StringSyntax(StringSyntaxAttribute.Uri)]
        string? requestUri,
        Dictionary<string, string> headers,
        HttpCompletionOption completionOption,
        CancellationToken cancellationToken)
    {
        using var request = CreateHttpRequestMessage(method, requestUri, headers);
        using var response = await client.SendAsync(request, completionOption, cancellationToken);
        await EnsureSuccessStatusCodeAsync(response);
        return  await response.Content.ReadAsStringAsync(cancellationToken);
    }

    public async  Task<string?> SendAndReadAsStringAsync<TIn>(
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
        await EnsureSuccessStatusCodeAsync(response);
        return  await response.Content.ReadAsStringAsync(cancellationToken);
    }

    public async  Task<TOut> SendAndDeserializeAsync<TOut>(
        HttpClient client,
        HttpMethod method,
        [StringSyntax(StringSyntaxAttribute.Uri)] string? requestUri,
        Dictionary<string, string> headers,
        HttpCompletionOption completionOption,
        JsonSerializerOptions? options,
        CancellationToken cancellationToken)
    {
        using var request = CreateHttpRequestMessage(method, requestUri, headers);
        using var response = await client.SendAsync(request, completionOption, cancellationToken);
        await EnsureSuccessStatusCodeAsync(response);
        return await ReadContentAsync<TOut>(options, cancellationToken, response);

    }

    public async  Task<TOut> SendAndDeserializeAsync<TIn,TOut>(
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
        await EnsureSuccessStatusCodeAsync(response);
        return await ReadContentAsync<TOut>(options, cancellationToken, response);
    }

    public async  IAsyncEnumerable<StreamItem<TOut>> SendAndReadStreamAsync<TIn,TOut>(
        TIn requestContent,
        HttpClient client,
        HttpMethod method,
        [StringSyntax(StringSyntaxAttribute.Uri)] string? requestUri,
        Dictionary<string, string> headers,
        HttpCompletionOption completionOption,
        JsonSerializerOptions? options,
        [EnumeratorCancellation] CancellationToken cancellationToken,
        Func<StreamReader,CancellationToken, Task<StreamItem<TOut>>> contentReaderAsync)
    {

        using var request = CreateHttpRequestMessage(method, requestUri, requestContent, headers, options);
        using var response = await client.SendAsync(request, completionOption, cancellationToken);
        await EnsureSuccessStatusCodeAsync(response);
        await foreach (var line in ReadContentStreamAsync(cancellationToken, response, contentReaderAsync))
        {
            yield return line;
        }
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

    private static HttpRequestMessage CreateHttpRequestMessage(HttpMethod method, string? requestUri,
        Dictionary<string, string> headers)
    {
        HttpRequestMessage? request = null;
        try
        {
            request = new HttpRequestMessage(method, requestUri);

            foreach (var (name, value) in headers)
            {
                request.Headers.Add(name, value);
            }
            return request;
        }
        catch
        {
            request?.Dispose();
            throw;
        }
    }

    private async Task<TOut> ReadContentAsync<TOut>(JsonSerializerOptions? options,
        CancellationToken cancellationToken, HttpResponseMessage response)
    {
#if DEBUG
        return await ReadContentInDebugAsync<TOut>(options, cancellationToken, response);
#else
        return await ReadContentInReleaseAsync<TOut>(options, cancellationToken, response);
#endif
    }


#if RELEASE
    private static async Task<TOut> ReadContentInReleaseAsync<TOut>(JsonSerializerOptions? options,
        CancellationToken cancellationToken, HttpResponseMessage response)
    {
        await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        // In release, deserialize directly from the stream for efficiency
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
#endif
#if DEBUG
    private async Task<TOut> ReadContentInDebugAsync<TOut>(JsonSerializerOptions? options,
        CancellationToken cancellationToken, HttpResponseMessage response)
    {
        await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        // Buffer the stream to memory for logging and deserialization only in debug builds
        using var memoryStream = new MemoryStream();
        if (response.Content.Headers.ContentEncoding.Contains("gzip"))
        {
            await using var gzipStream = new GZipStream(responseStream, CompressionMode.Decompress);
            await gzipStream.CopyToAsync(memoryStream, cancellationToken);
        }
        else
        {
            await responseStream.CopyToAsync(memoryStream, cancellationToken);
        }
        memoryStream.Seek(0, SeekOrigin.Begin);
        var contentString = Encoding.UTF8.GetString(memoryStream.ToArray());
        logger.LogInformation("Response stream content: {Content}", contentString);
        memoryStream.Seek(0, SeekOrigin.Begin);
        var result = await JsonSerializer.DeserializeAsync<TOut>(memoryStream, options, cancellationToken);
        return result!;
    }
#endif
    private static async IAsyncEnumerable<StreamItem<TOut>> ReadContentStreamAsync<TOut>(
        [EnumeratorCancellation] CancellationToken cancellationToken,
        HttpResponseMessage response,
        Func<StreamReader,CancellationToken, Task<StreamItem<TOut>>> contentReaderAsync)
    {
        await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(responseStream);

        while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
        {
            yield return await contentReaderAsync(reader,cancellationToken);
        }
    }

    private async Task EnsureSuccessStatusCodeAsync(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = $"Request failed with status code {response.StatusCode}.";
            logger.LogError("{@ErrorMessage} Content Is {@Content} and {@Reason}",errorMessage, await response.Content.ReadAsStringAsync(), response.ReasonPhrase);
            throw new HttpRequestException(errorMessage);
        }
    }
}
