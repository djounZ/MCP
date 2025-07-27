using System.Net;
using System.Text;
using System.Text.Json;

namespace AI.GithubCopilot.Tests.Infrastructure.Services;

/// <summary>
/// A mock HttpMessageHandler that can be used to test HTTP client functionality
/// without making actual network calls.
/// </summary>
public class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _handlerFunc;
    private HttpRequestMessage? _lastRequest;

    /// <summary>
    /// Gets the last request that was sent to this handler.
    /// </summary>
    public HttpRequestMessage? LastRequest => _lastRequest;
    
    /// <summary>
    /// Creates a new instance of the <see cref="MockHttpMessageHandler"/> class that returns
    /// a predefined response.
    /// </summary>
    /// <param name="statusCode">The HTTP status code to return.</param>
    public MockHttpMessageHandler(HttpStatusCode statusCode = HttpStatusCode.OK)
        : this(new StringContent(string.Empty), statusCode)
    {
    }

    /// <summary>
    /// Creates a new instance of the <see cref="MockHttpMessageHandler"/> class that returns
    /// a response with the given content.
    /// </summary>
    /// <param name="content">The content to include in the response.</param>
    /// <param name="statusCode">The HTTP status code to return.</param>
    public MockHttpMessageHandler(HttpContent content, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        _handlerFunc = (request, cancellationToken) =>
        {
            var response = new HttpResponseMessage(statusCode)
            {
                Content = content
            };
            return Task.FromResult(response);
        };
    }

    /// <summary>
    /// Creates a new instance of the <see cref="MockHttpMessageHandler"/> class with a custom handler function.
    /// </summary>
    /// <param name="handlerFunc">A function that processes the request and returns a response.</param>
    public MockHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handlerFunc)
    {
        _handlerFunc = handlerFunc;
    }

    /// <summary>
    /// Creates a mock handler that returns a JSON response.
    /// </summary>
    /// <typeparam name="T">The type of content to serialize.</typeparam>
    /// <param name="content">The content to include in the response.</param>
    /// <param name="statusCode">The HTTP status code for the response.</param>
    /// <param name="useGzip">Whether to compress the response with gzip.</param>
    /// <param name="options">JSON serialization options.</param>
    /// <returns>A configured mock handler.</returns>
    public static MockHttpMessageHandler CreateWithJsonContent<T>(
        T content,
        HttpStatusCode statusCode = HttpStatusCode.OK,
        bool useGzip = false,
        JsonSerializerOptions? options = null)
    {
        return new MockHttpMessageHandler((request, cancellationToken) =>
        {
            var json = JsonSerializer.Serialize(content, options);
            HttpContent responseContent;

            if (useGzip)
            {
                // Create gzipped content
                byte[] compressedBytes;
                using (var ms = new MemoryStream())
                {
                    using (var gzip = new System.IO.Compression.GZipStream(ms, System.IO.Compression.CompressionMode.Compress, true))
                    using (var writer = new StreamWriter(gzip, Encoding.UTF8))
                    {
                        writer.Write(json);
                        writer.Flush();
                    }
                    compressedBytes = ms.ToArray();
                }

                responseContent = new ByteArrayContent(compressedBytes);
                responseContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                responseContent.Headers.ContentEncoding.Add("gzip");
            }
            else
            {
                // Create regular JSON content
                responseContent = new StringContent(json, Encoding.UTF8, "application/json");
            }

            var response = new HttpResponseMessage(statusCode)
            {
                Content = responseContent
            };
            
            return Task.FromResult(response);
        });
    }

    /// <summary>
    /// Processes an HTTP request message.
    /// </summary>
    /// <param name="request">The HTTP request message to send.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous send operation. The task result contains the HTTP response message.</returns>
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        _lastRequest = request;
        
        // Call the handler function to get a new response for each request
        return _handlerFunc(request, cancellationToken);
    }
}