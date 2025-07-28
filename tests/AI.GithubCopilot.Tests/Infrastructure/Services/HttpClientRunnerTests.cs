using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;
using System.Text.Json;
using AI.GithubCopilot.Infrastructure.Services;

namespace AI.GithubCopilot.Tests.Infrastructure.Services;

/// <summary>
/// Tests for the <see cref="HttpClientRunner"/> class.
/// </summary>
public class HttpClientRunnerTests
{
    // Shared test objects
    private static readonly TestRequest DefaultRequestData = new() { Name = "Test", Value = 42 };
    private static readonly TestResponse DefaultResponseData = new() { Success = true, Message = "Success" };
    private static readonly TestResponse GzipResponseData = new() { Success = true, Message = "GzipSuccess" };
    private static readonly TestResponse NullRequestResponseData = new() { Success = true, Message = "NullRequest" };

    // Shared headers dictionary
    private static readonly Dictionary<string, string> EmptyHeaders = new();
    private static readonly Dictionary<string, string> TestHeaders = new()
    {
        { "X-Test-Header", "TestValue" }
    };

    // Common error messages
    private const string InvalidGzipMessage = "Not actually gzipped content";
    private const string InvalidJsonMessage = "{ invalid json";

    // Shared test configuration
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    // Shared HttpClientRunner instance
    private readonly HttpClientRunner _runner = new();

    /// <summary>
    /// Tests that SendAsyncAndDeserialize correctly sends a request and deserializes the response.
    /// </summary>
    [Fact]
    public async Task SendAsyncAndDeserialize_SuccessfulRequest_ReturnsDeserializedResponse()
    {
        // Arrange - Use shared test objects
        // Capture the request content and headers for verification
        string? capturedRequestContent = null;
        Dictionary<string, string> capturedHeaders = new();
        HttpMethod? capturedMethod = null;
        string? capturedUri = null;

        var mockHandler = new MockHttpMessageHandler((request, t) =>
        {
            // Capture request details before they're disposed
            capturedMethod = request.Method;
            capturedUri = request.RequestUri?.ToString();

            // Capture headers efficiently
            foreach (var header in request.Headers.Where(h => h.Value.Any()))
            {
                capturedHeaders[header.Key] = header.Value.First();
            }

            // Capture content
            if (request.Content != null)
            {
                capturedRequestContent = request.Content.ReadAsStringAsync(t).GetAwaiter().GetResult();
            }

            return Task.FromResult(CreateResponse());
        });

        // Act - Create the client in the main method scope since we need to access the result
        TestResponse? result;

        using (var httpClient = new HttpClient(mockHandler))
        {
            // Run the test within the using scope to ensure proper disposal
            result = await _runner.SendAndDeserializeAsync<TestRequest, TestResponse>(
                DefaultRequestData,
                httpClient,
                HttpMethod.Post,
                "https://example.com/api/test",
                TestHeaders,
                HttpCompletionOption.ResponseContentRead,
                _jsonOptions,
                CancellationToken.None);
        }

        // Assert - After client is disposed, we can safely check the captured values
        capturedMethod.Should().Be(HttpMethod.Post);
        capturedUri.Should().Be("https://example.com/api/test");
        capturedHeaders.Should().ContainKey("X-Test-Header");
        capturedHeaders["X-Test-Header"].Should().Be("TestValue");

        // Check request content
        var expectedJson = JsonSerializer.Serialize(DefaultRequestData, _jsonOptions);
        capturedRequestContent.Should().Be(expectedJson);

        // Check result - with records, we can compare the entire object
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(DefaultResponseData);
        return;

        // Create a reusable function for response creation to reduce allocations
        HttpResponseMessage CreateResponse() =>
            new(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(DefaultResponseData, _jsonOptions),
                    Encoding.UTF8, "application/json")
            };
    }

    /// <summary>
    /// Tests that SendAsyncAndDeserialize correctly handles gzipped responses.
    /// </summary>
    [Fact]
    public async Task SendAsyncAndDeserialize_GzippedResponse_DeserializesCorrectly()
    {
        // Arrange - Using shared objects
        var mockHandler = MockHttpMessageHandler.CreateWithJsonContent(
            GzipResponseData,
            useGzip: true,
            options: _jsonOptions);

        // Act - Create the client in the main method scope since we need to access the result
        TestResponse? result;

        using (var httpClient = new HttpClient(mockHandler))
        {
            // Run the test within the using scope to ensure proper disposal
            result = await _runner.SendAndDeserializeAsync<TestRequest, TestResponse>(
                DefaultRequestData,
                httpClient,
                HttpMethod.Get,
                "https://example.com/api/gzip-test",
                EmptyHeaders,
                HttpCompletionOption.ResponseContentRead,
                _jsonOptions,
                CancellationToken.None);
        }

        // Assert - After client is disposed
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(GzipResponseData);
    }

    /// <summary>
    /// Tests that SendAsyncAndDeserialize throws an exception when the response is not successful.
    /// </summary>
    [Fact]
    public async Task SendAsyncAndDeserialize_ErrorResponse_ThrowsException()
    {
        // Arrange - Using shared objects and error content that can be reused
        using var errorContent = new StringContent("{ \"error\": \"Bad request\" }", Encoding.UTF8, "application/json");
        var mockHandler = new MockHttpMessageHandler(errorContent, HttpStatusCode.BadRequest);

        // Act & Assert - Create the client inside the act function to prevent disposal issues
        await FluentActions.Invoking(async () =>
        {
            // Create client inside the delegate to avoid disposal issues
            using var localHttpClient = new HttpClient(mockHandler);

            await _runner.SendAndDeserializeAsync<TestRequest, TestResponse>(
                DefaultRequestData,
                localHttpClient,
                HttpMethod.Post,
                "https://example.com/api/error",
                EmptyHeaders,
                HttpCompletionOption.ResponseContentRead,
                _jsonOptions,
                CancellationToken.None);
        }).Should().ThrowAsync<HttpRequestException>()
            .Where(ex => ex.StatusCode == HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Tests that SendAsyncAndDeserialize handles cancellation correctly.
    /// </summary>
    [Fact]
    public async Task SendAsyncAndDeserialize_CancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange - Using shared objects
        // Create a mock handler that returns a delayed response
        using var delayedContent = new StringContent("{ \"success\": true, \"message\": \"Delayed\" }", Encoding.UTF8, "application/json");
        var mockHandler = new MockHttpMessageHandler(delayedContent);


        // Act & Assert - Create the client inside the act function to prevent disposal issues
        await FluentActions.Invoking(async () =>
        {
            // Create client inside the delegate to avoid disposal issues
            using var localHttpClient = new HttpClient(mockHandler);

            // Create a cancellation token that's already canceled
            using var cts = new CancellationTokenSource();
            await cts.CancelAsync();

            await _runner.SendAndDeserializeAsync<TestRequest, TestResponse>(
                DefaultRequestData,
                localHttpClient,
                HttpMethod.Get,
                "https://example.com/api/canceled",
                EmptyHeaders,
                HttpCompletionOption.ResponseContentRead,
                _jsonOptions,
                cts.Token);
        }).Should().ThrowAsync<OperationCanceledException>();
    }

    /// <summary>
    /// Tests that SendAsyncAndDeserialize correctly handles null values in the request content.
    /// </summary>
    [Fact]
    public async Task SendAsyncAndDeserialize_NullRequestContent_SerializesCorrectly()
    {
        // Arrange - We'll need to capture request content
        string? capturedRequestContent = null;
        TestResponse? capturedResult = null;

        var mockHandler = new MockHttpMessageHandler((request, t) =>
        {
            // Capture the request content before it's disposed
            if (request.Content != null)
            {
                capturedRequestContent = request.Content.ReadAsStringAsync(t).GetAwaiter().GetResult();
            }

            // Return a successful response with our shared NullRequestResponseData
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(NullRequestResponseData, _jsonOptions),
                    Encoding.UTF8, "application/json")
            };
            return Task.FromResult(response);
        });

        // Act - Use FluentActions.Invoking pattern for consistency
        await FluentActions.Invoking(async () =>
        {
            // Create client inside the delegate to avoid disposal issues
            using var localHttpClient = new HttpClient(mockHandler);

            // Store the result to assert later
            capturedResult = await _runner.SendAndDeserializeAsync<TestRequest?, TestResponse>(
                null, // Null request data
                localHttpClient,
                HttpMethod.Post,
                "https://example.com/api/null-test",
                EmptyHeaders,
                HttpCompletionOption.ResponseContentRead,
                _jsonOptions,
                CancellationToken.None);

            // Capture the request info before disposal
            mockHandler.LastRequest.Should().NotBeNull();
        }).Should().NotThrowAsync(); // We expect no exceptions

        // Assert - After client is disposed, verify the captured data
        capturedRequestContent.Should().Be("null");
        capturedResult.Should().NotBeNull();
        capturedResult.Should().BeEquivalentTo(NullRequestResponseData);
    }

    /// <summary>
    /// Tests that SendAsyncAndDeserialize handles errors in request creation correctly.
    /// </summary>
    [Fact]
    public async Task SendAsyncAndDeserialize_InvalidHeader_ThrowsException()
    {
        // Arrange - Using shared objects, but with invalid headers
        // Create a handler that will stay in scope for the entire test
        var mockHandler = new MockHttpMessageHandler();

        // Create an invalid header that will cause HttpRequestMessage.Headers.Add to throw
        var invalidHeaders = new Dictionary<string, string>
        {
            { "Invalid:Header", "Value with invalid characters" }
        };

        // Act & Assert - Create the client inside the act function to prevent disposal issues
        await FluentActions.Invoking(async () =>
        {
            // Create client inside the delegate to avoid disposal issues
            using var localHttpClient = new HttpClient(mockHandler);

            await _runner.SendAndDeserializeAsync<TestRequest, TestResponse>(
                DefaultRequestData,
                localHttpClient,
                HttpMethod.Post,
                "https://example.com/api/invalid",
                invalidHeaders,
                HttpCompletionOption.ResponseContentRead,
                _jsonOptions,
                CancellationToken.None);
        }).Should().ThrowAsync<FormatException>();
    }

    /// <summary>
    /// Tests that SendAsyncAndDeserialize handles JSON deserialization errors correctly.
    /// </summary>
    [Fact]
    public async Task SendAsyncAndDeserialize_InvalidJsonResponse_ThrowsException()
    {
        // Arrange - Using shared objects with invalid JSON content
        using var invalidContent = new StringContent(InvalidJsonMessage, Encoding.UTF8, "application/json");
        var mockHandler = new MockHttpMessageHandler(invalidContent);

        // Act & Assert - Create the client inside the act function to prevent disposal issues
        await FluentActions.Invoking(async () =>
        {
            // Create client inside the delegate to avoid disposal issues
            using var localHttpClient = new HttpClient(mockHandler);

            await _runner.SendAndDeserializeAsync<TestRequest, TestResponse>(
                DefaultRequestData,
                localHttpClient,
                HttpMethod.Get,
                "https://example.com/api/invalid-json",
                EmptyHeaders,
                HttpCompletionOption.ResponseContentRead,
                _jsonOptions,
                CancellationToken.None);
        }).Should().ThrowAsync<JsonException>();
    }

    /// <summary>
    /// Tests that SendAsyncAndDeserialize handles invalid gzipped content correctly.
    /// </summary>
    [Fact]
    public async Task SendAsyncAndDeserialize_InvalidGzipContent_ThrowsException()
    {
        // Arrange - Using shared objects with invalid gzip content
        using var invalidGzipContent = new StringContent(InvalidGzipMessage, Encoding.UTF8, "application/json");
        invalidGzipContent.Headers.ContentEncoding.Add("gzip");
        var mockHandler = new MockHttpMessageHandler(invalidGzipContent);

        // Act & Assert - Create the client inside the act function to prevent disposal issues
        await FluentActions.Invoking(async () =>
        {
            // Create client inside the delegate to avoid disposal issues
            using var localHttpClient = new HttpClient(mockHandler);

            await _runner.SendAndDeserializeAsync<TestRequest, TestResponse>(
                DefaultRequestData,
                localHttpClient,
                HttpMethod.Get,
                "https://example.com/api/invalid-gzip",
                EmptyHeaders,
                HttpCompletionOption.ResponseContentRead,
                _jsonOptions,
                CancellationToken.None);
        }).Should().ThrowAsync<InvalidDataException>()
            .WithMessage("*unsupported compression*");
    }
}

/// <summary>
/// Test request class for use in HttpClientRunner tests.
/// </summary>
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public record TestRequest
{
    public string? Name { get; set; }
    public int Value { get; set; }
}

/// <summary>
/// Test response class for use in HttpClientRunner tests.
/// </summary>
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public record TestResponse
{
    public bool Success { get; init; }
    public string? Message { get; init; }
}
