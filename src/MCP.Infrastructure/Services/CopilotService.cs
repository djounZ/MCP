

using System.Text;
using System.Text.Json;
using System.IO;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MCP.Domain.Interfaces;
using MCP.Infrastructure.Constants;
using CopilotServiceOptions = MCP.Infrastructure.Options.CopilotServiceOptions;

namespace MCP.Infrastructure.Services
{
    public class CopilotService : ICopilotService, IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly Timer _tokenRefreshTimer;
        private readonly CopilotServiceOptions _options;
        private string? _token;
        private bool _disposed;

        public CopilotService(HttpClient httpClient, CopilotServiceOptions options)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _options = options ?? throw new ArgumentNullException(nameof(options));

            // Start token refresh timer
            _tokenRefreshTimer = new Timer(async _ =>
            {
                if (!_disposed)
                {
                    try
                    {
                        await GetTokenAsync();
                    }
                    catch (ObjectDisposedException)
                    {
                        // HttpClient was disposed, ignore
                    }
                }
            }, null, TimeSpan.Zero, TimeSpan.FromMinutes(25));
        }

    public async Task SetupAsync()
    {
        var requestData = new
        {
            client_id = _options.ClientId,
            scope = "read:user"
        };

        var headers = new Dictionary<string, string>
        {
            [HeaderKeys.Accept] = ContentTypes.ApplicationJson,
            [HeaderKeys.EditorVersion] = _options.EditorVersion,
            [HeaderKeys.EditorPluginVersion] = _options.EditorPluginVersion,
            [HeaderKeys.ContentType] = ContentTypes.ApplicationJson,
            [HeaderKeys.UserAgent] = _options.UserAgent,
            [HeaderKeys.AcceptEncoding] = _options.AcceptEncoding
        };


        var response = await SendPostRequestAsync(_options.DeviceCodeUrl, requestData, headers);
        var deviceCodeResponse = System.Text.Json.JsonSerializer.Deserialize<Models.Copilot.CopilotDeviceCodeResponse>(response);
        if (deviceCodeResponse == null)
            throw new InvalidOperationException("Failed to parse device code response.");

        Console.WriteLine($"Please visit {deviceCodeResponse.VerificationUri} and enter code {deviceCodeResponse.UserCode} to authenticate.");


        string? accessToken = null;
        while (accessToken == null)
        {
            await Task.Delay(5000);

            var tokenRequestData = new
            {
                client_id = _options.ClientId,
                device_code = deviceCodeResponse.DeviceCode,
                grant_type = "urn:ietf:params:oauth:grant-type:device_code"
            };

            var tokenResponse = await SendPostRequestAsync(_options.AccessTokenUrl, tokenRequestData, headers);
            using var tokenResponseJsonDoc = JsonDocument.Parse(tokenResponse);
            var tokenRoot = tokenResponseJsonDoc.RootElement;
            if (tokenRoot.TryGetProperty("access_token", out var accessTokenProp))
            {
                accessToken = accessTokenProp.GetString();
            }
        }

        await File.WriteAllTextAsync(".copilot_token", accessToken);
        Console.WriteLine("Authentication success!");
    }

    public async Task GetTokenAsync()
    {
        string accessToken;
        while (true)
        {
            try
            {
                accessToken = await File.ReadAllTextAsync(".copilot_token");
                break;
            }
            catch (FileNotFoundException)
            {
                await SetupAsync();
            }
        }

        var headers = new Dictionary<string, string>
        {
            [HeaderKeys.Authorization] = $"token {accessToken}",
            [HeaderKeys.EditorVersion] = _options.EditorVersion,
            [HeaderKeys.EditorPluginVersion] = _options.EditorPluginVersion,
            [HeaderKeys.UserAgent] = _options.UserAgent
        };

        var response = await SendGetRequestAsync(_options.TokenUrl, headers);
        using var responseJsonDoc = JsonDocument.Parse(response);
        var responseRoot = responseJsonDoc.RootElement;
        if (responseRoot.TryGetProperty("token", out var tokenProp))
        {
            _token = tokenProp.GetString();
        }
    }

    public async Task<string> GetCompletionAsync(string prompt, string language = "python")
    {
        if (_token == null || IsTokenInvalid(_token))
        {
            await GetTokenAsync();
        }

        var requestData = new
        {
            prompt = prompt,
            suffix = "",
            max_tokens = 1000,
            temperature = 0,
            top_p = 1,
            n = 1,
            stop = new[] { "\n" },
            nwo = "github/copilot.vim",
            stream = true,
            extra = new { language = language }
        };

        var headers = new Dictionary<string, string>
        {
            [HeaderKeys.Authorization] = $"Bearer {_token}"
        };

        try
        {
            var response = await SendPostRequestAsync(_options.CompletionUrl, requestData, headers);
            return ParseStreamingResponse(response);
        }
        catch (HttpRequestException)
        {
            return "";
        }
    }

    private string ParseStreamingResponse(string responseText)
    {
        var result = new StringBuilder();
        var lines = responseText.Split('\n');

        foreach (var line in lines)
        {
            if (line.StartsWith("data: {"))
            {
                try
                {
                    using var jsonDoc = JsonDocument.Parse(line.Substring(6));
                    var root = jsonDoc.RootElement;
                    if (root.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
                    {
                        var firstChoice = choices[0];
                        if (firstChoice.TryGetProperty("text", out var textProperty))
                        {
                            var completion = textProperty.GetString();
                            if (!string.IsNullOrEmpty(completion))
                            {
                                result.Append(completion);
                            }
                            else
                            {
                                result.Append('\n');
                            }
                        }
                        else
                        {
                            // No text property, likely a completion marker
                            result.Append('\n');
                        }
                    }
                    else
                    {
                        result.Append('\n');
                    }
                }
                catch (JsonException)
                {
                    // Skip invalid JSON lines
                }
            }
        }

        return result.ToString();
    }

    private bool IsTokenInvalid(string token)
    {
        if (string.IsNullOrEmpty(token) || !token.Contains("exp"))
        {
            return true;
        }

        var expValue = ExtractExpValue(token);
        return expValue <= DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    private long ExtractExpValue(string token)
    {
        var pairs = token.Split(';');
        foreach (var pair in pairs)
        {
            var keyValue = pair.Split('=');
            if (keyValue.Length == 2 && keyValue[0].Trim() == "exp")
            {
                if (long.TryParse(keyValue[1].Trim(), out var exp))
                {
                    return exp;
                }
            }
        }
        return 0;
    }

    private async Task<string> SendPostRequestAsync(string url, object data, Dictionary<string, string> headers)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, url);

        foreach (KeyValuePair<string, string> header in headers)
        {
            if (header.Key == HeaderKeys.ContentType)
                continue; // This will be set by StringContent
            request.Headers.Add(header.Key, header.Value);
        }

        string json = JsonSerializer.Serialize(data);
        request.Content = new StringContent(json, Encoding.UTF8, ContentTypes.ApplicationJson);

        using HttpResponseMessage response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        await using Stream responseStream = await response.Content.ReadAsStreamAsync();
        Stream contentStream = response.Content.Headers.ContentEncoding.Contains(ContentEncodings.Gzip)
            ? new System.IO.Compression.GZipStream(responseStream, System.IO.Compression.CompressionMode.Decompress)
            : responseStream;

        using var reader = new StreamReader(contentStream, Encoding.UTF8);
        string result = await reader.ReadToEndAsync();

        if (contentStream is System.IO.Compression.GZipStream gzipStream)
            await gzipStream.DisposeAsync();

        return result;
    }

    private async Task<string> SendGetRequestAsync(string url, Dictionary<string, string> headers)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);

        foreach (var header in headers)
        {
            request.Headers.Add(header.Key, header.Value);
        }

        var response = await _httpClient.SendAsync(request);
        return await response.Content.ReadAsStringAsync();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _tokenRefreshTimer?.Dispose();
            _disposed = true;
        }
    }

    }
}
