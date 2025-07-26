using System.IO.Compression;
using System.Security;
using System.Text;
using System.Text.Json;
using MCP.Application.Interfaces;
using MCP.Application.Models;
using MCP.Domain.Common;
using MCP.Infrastructure.Constants;
using MCP.Infrastructure.Models;
using MCP.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MCP.Infrastructure.Services;

public sealed class CopilotService : ICopilotService, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CopilotService> _logger;
    private readonly CopilotServiceState _state;
    private readonly CopilotServiceOptions _options;
    private readonly Timer _tokenRefreshTimer;
    private readonly string _tokenFilePath;
    private bool _disposed;
    private byte[]? _encryptedToken;


    public CopilotService(HttpClient httpClient, IOptions<CopilotServiceOptions> options, ILogger<CopilotService> logger, CopilotServiceState state)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _state = state;
        _tokenFilePath = Path.Combine(".copilot_token_encrypted");

        // Start token refresh timer
        _tokenRefreshTimer = new Timer(_ =>
        {
            try
            {
                /*if (!_disposed)
                {
                    var tokenResult = await GetTokenInternalAsync();
                    if (tokenResult.IsFailure)
                    {
                        _logger.LogWarning("Token refresh failed: {Error}", tokenResult.Error);
                    }
                }*/
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during token refresh timer callback.");
            }
        }, null, TimeSpan.Zero, TimeSpan.FromMinutes(25));

    }

    public async Task<Result<CopilotDeviceCodeResponse>> RegisterDeviceAsync()
    {
        if (_state.IsValid)
        {
            return Result<CopilotDeviceCodeResponse>.Success(MapToApplicationModel(_state.GithubDeviceCodeResponse));
        }
        var headers = new Dictionary<string, string>
        {
            [HeaderKeys.Accept] = ContentTypes.ApplicationJson,
            [HeaderKeys.EditorVersion] = _options.EditorVersion,
            [HeaderKeys.EditorPluginVersion] = _options.EditorPluginVersion,
            [HeaderKeys.ContentType] = ContentTypes.ApplicationJson,
            [HeaderKeys.UserAgent] = _options.UserAgent,
            [HeaderKeys.AcceptEncoding] = _options.AcceptEncoding
        };
        var deviceCodeUrl = _options.DeviceCodeUrl;
        var clientId = _options.ClientId;
        var scope = "read:user";
        var requestData = new { client_id = clientId, scope = scope };
        var githubResult =   await SendPostRequestAsync<object,GithubDeviceCodeResponse>(deviceCodeUrl, requestData, headers);
        return githubResult.Match(
            onSuccess: githubResponse =>
            {
                 _state.GithubDeviceCodeResponse = githubResponse;
                return Result.Success(MapToApplicationModel(githubResponse));
            },
            onFailure: Result.Failure<CopilotDeviceCodeResponse>
        );
    }

    public async Task<Result<Unit>> GetAccessTokenAsync()
    {
        if (!string.IsNullOrWhiteSpace(_state.GithubAccessTokenResponse.AccessToken)) return Result<Unit>.Success(Unit.Value);
        var headers = new Dictionary<string, string>
        {
            [HeaderKeys.Accept] = ContentTypes.ApplicationJson,
            [HeaderKeys.EditorVersion] = _options.EditorVersion,
            [HeaderKeys.EditorPluginVersion] = _options.EditorPluginVersion,
            [HeaderKeys.ContentType] = ContentTypes.ApplicationJson,
            [HeaderKeys.UserAgent] = _options.UserAgent
        };

        var tokenRequestData = new
        {
            client_id = _options.ClientId,
            device_code = _state.GithubDeviceCodeResponse.DeviceCode,
            grant_type = "urn:ietf:params:oauth:grant-type:device_code"
        };

        var requestAsync = await SendPostRequestAsync<object,GithubAccessTokenResponse>(_options.GithubTokenUrl, tokenRequestData, headers);
        return await requestAsync.MatchAsync(
            onSuccess:  async response => await _state.SetAccessToken(response),
            onFailure: error => Result.Failure<Unit>($"Failed to get access token: {error}")
        );
    }

    private async Task<Result<GithubDeviceCodeResponse>> GetGithubDeviceCodeResponseAsync(string deviceCodeUrl,string clientId, string scope, Dictionary<string, string> headers)
    {
        var requestData = new { client_id = clientId, scope = scope };
        return  await SendPostRequestAsync<object,GithubDeviceCodeResponse>(deviceCodeUrl, requestData, headers);
    }

    public async Task<Result<Unit>> SetupAsync()
    {
        var headers = new Dictionary<string, string>
        {
            [HeaderKeys.Accept] = ContentTypes.ApplicationJson,
            [HeaderKeys.EditorVersion] = _options.EditorVersion,
            [HeaderKeys.EditorPluginVersion] = _options.EditorPluginVersion,
            [HeaderKeys.ContentType] = ContentTypes.ApplicationJson,
            [HeaderKeys.UserAgent] = _options.UserAgent,
            [HeaderKeys.AcceptEncoding] = _options.AcceptEncoding
        };
        var deviceCodeUrl = _options.DeviceCodeUrl;
        var clientId = _options.ClientId;
        var scope = "read:user";
        var grantType = "urn:ietf:params:oauth:grant-type:device_code";
        try
        {
            var requestData = new { client_id = clientId, scope = scope };

            var responseResult = await SendPostRequestAsync(deviceCodeUrl, requestData, headers);
            if (responseResult.IsFailure)
            {
                return Result.Failure<Unit>($"Failed to get device code: {responseResult.Error}");
            }

            var githubDeviceCodeResponseAsync = await GetGithubDeviceCodeResponseAsync(deviceCodeUrl, clientId, scope, headers);


           return await githubDeviceCodeResponseAsync.MatchAsync(
                onSuccess: async deviceCodeResponse =>
                {
                    var deviceCode = deviceCodeResponse.DeviceCode;

                    return await GetGithubAccessTokenAsync(clientId, deviceCode, grantType, headers);
                },
                onFailure: Result.Failure<Unit>);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during setup.");
            return Result.Failure<Unit>($"Unexpected error during setup: {ex.Message}");
        }
    }

    private async Task<Result<Unit>> GetGithubAccessTokenAsync(string clientId, string deviceCode, string grantType, Dictionary<string, string> headers)
    {
        string? accessToken = null;
        while (accessToken == null)
        {
            await Task.Delay(5000);

            var tokenRequestData = new
            {
                client_id = clientId,
                device_code = deviceCode,
                grant_type = grantType
            };

            var tokenResponseResult =
                await SendPostRequestAsync(_options.GithubTokenUrl, tokenRequestData, headers);
            if (tokenResponseResult.IsFailure)
            {
                continue; // Keep trying
            }

            try
            {
                using var tokenResponseJsonDoc = JsonDocument.Parse(tokenResponseResult.Value);
                var tokenRoot = tokenResponseJsonDoc.RootElement;
                if (tokenRoot.TryGetProperty("access_token", out var accessTokenProp))
                {
                    accessToken = accessTokenProp.GetString();
                }
            }
            catch (JsonException)
            {
                // Continue trying
            }
        }

        try
        {
            await SaveTokenSecurelyAsync(accessToken);
            _logger.LogInformation("Authentication success! Token saved securely.");
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save token securely.");
            return Result.Failure<Unit>("Failed to save token securely.");
        }
    }

    public async Task<Result<string>> GetCompletionAsync(string prompt, string language = "python")
    {
        var currentTokenResult = await GetCurrentTokenAsync();
        if (currentTokenResult.IsFailure || IsTokenInvalid(currentTokenResult.Value))
        {
            var tokenResult = await GetTokenInternalAsync();
            if (tokenResult.IsFailure)
            {
                return Result.Failure<string>($"Failed to get token: {tokenResult.Error}");
            }
            currentTokenResult = await GetCurrentTokenAsync();
            if (currentTokenResult.IsFailure)
            {
                return Result.Failure<string>($"Failed to retrieve valid token: {currentTokenResult.Error}");
            }
        }

        var currentToken = currentTokenResult.Value;
        var requestData = new
        {
            prompt,
            suffix = "",
            max_tokens = 1000,
            temperature = 0,
            top_p = 1,
            n = 1,
            stop = new[] { "\n" },
            nwo = "github/copilot.vim",
            stream = true,
            extra = new { language }
        };

        var headers = new Dictionary<string, string> { [HeaderKeys.Authorization] = $"Bearer {currentToken}" };

        try
        {
            var responseResult = await SendPostRequestAsync(_options.CompletionUrl, requestData, headers);
            if (responseResult.IsFailure)
            {
                return Result.Failure<string>($"Failed to get completion: {responseResult.Error}");
            }

            var completion = ParseStreamingResponse(responseResult.Value);
            return Result.Success(completion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while requesting Copilot completion.");
            return Result.Failure<string>($"Error occurred while requesting Copilot completion: {ex.Message}");
        }
        finally
        {
            // Clear the token from memory
            if (currentToken != null)
            {
                EnvHelpers.SecureClearString(currentToken);
            }
        }
    }

    public bool HasAccessToken => _state.IsValid;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private async Task<Result<Unit>> GetTokenInternalAsync()
    {
        var accessToken = string.Empty;
        while (true)
        {
            var tokenResult = await LoadTokenSecurelyAsync();
            var shouldBreak = tokenResult.Match(
                onSuccess: token =>
                {
                    accessToken = token;
                    return true;
                },
                onFailure: error => false);

            if (shouldBreak)
            {
                break;
            }

            // Token not found or invalid, need to setup
            var setupResult = await SetupAsync();
            if (setupResult.IsFailure)
            {
                return setupResult;
            }
        }

        var headers = new Dictionary<string, string>
        {
            [HeaderKeys.Authorization] = $"token {accessToken}",
            [HeaderKeys.EditorVersion] = _options.EditorVersion,
            [HeaderKeys.EditorPluginVersion] = _options.EditorPluginVersion,
            [HeaderKeys.UserAgent] = _options.UserAgent
        };

        try
        {
            var responseResult = await SendGetRequestAsync(_options.GithubCopilotTokenUrl, headers);
            if (responseResult.IsFailure)
            {
                return Result.Failure<Unit>($"Failed to get token: {responseResult.Error}");
            }

            try
            {
                using var responseJsonDoc = JsonDocument.Parse(responseResult.Value);
                var responseRoot = responseJsonDoc.RootElement;
                if (responseRoot.TryGetProperty("token", out var tokenProp))
                {
                    var token = tokenProp.GetString();
                    if (!string.IsNullOrEmpty(token))
                    {
                        await StoreTokenSecurelyAsync(token);
                        _logger.LogInformation("Token successfully retrieved from Copilot API.");
                        return Result.Success();
                    }
                }

                _logger.LogWarning("Token property not found in Copilot API response.");
                return Result.Failure<Unit>("Token property not found in API response");
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse token response");
                return Result.Failure<Unit>("Failed to parse token response");
            }
        }
        finally
        {
            // Clear the access token from memory
            EnvHelpers.SecureClearString(accessToken);
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


    private async Task<Result<string>> SendPostRequestAsync<TIn>(string url, TIn data, Dictionary<string, string> headers)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, url);

            foreach (var header in headers.Where(header => header.Key != HeaderKeys.ContentType))
            {
                request.Headers.Add(header.Key, header.Value);
            }

            var json = JsonSerializer.Serialize(data);
            request.Content = new StringContent(json, Encoding.UTF8, ContentTypes.ApplicationJson);

            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            return !response.IsSuccessStatusCode ? Result.Failure<string>($"HTTP request failed with status code: {response.StatusCode}") : Result<string>.Success( await response.Content.ReadAsStringAsync());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in SendPostRequestAsync");
            return Result.Failure<string>($"Unexpected error", ex);
        }
    }

    private async Task<Result<TOut>> SendPostRequestAsync<TIn,TOut>(string url, TIn data, Dictionary<string, string> headers)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, url);

            foreach (var header in headers.Where(header => header.Key != HeaderKeys.ContentType))
            {
                request.Headers.Add(header.Key, header.Value);
            }

            var json = JsonSerializer.Serialize(data);
            request.Content = new StringContent(json, Encoding.UTF8, ContentTypes.ApplicationJson);

            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            if (!response.IsSuccessStatusCode)
            {
                return Result.Failure<TOut>($"HTTP request failed with status code: {response.StatusCode}");
            }

            await using var responseStream = await response.Content.ReadAsStreamAsync();
            var contentStream = response.Content.Headers.ContentEncoding.Contains(ContentEncodings.Gzip)
                ? new GZipStream(responseStream, CompressionMode.Decompress)
                : responseStream;

            using var reader = new StreamReader(contentStream, Encoding.UTF8);
            var result = await reader.ReadToEndAsync();

            if (contentStream is GZipStream gzipStream)
            {
                await gzipStream.DisposeAsync();
            }

            var deserialize = JsonSerializer.Deserialize<TOut>(result);
            return deserialize != null ? Result<TOut>.Success(deserialize) : Result.Failure<TOut>("Failed to deserialize response");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in SendPostRequestAsync");
            return Result.Failure<TOut>($"Unexpected error", ex);
        }
    }

    private async Task<Result<string>> SendPostRequestAsync(string url, object data, Dictionary<string, string> headers)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, url);

            foreach (var header in headers.Where(header => header.Key != HeaderKeys.ContentType))
            {
                request.Headers.Add(header.Key, header.Value);
            }

            var json = JsonSerializer.Serialize(data);
            request.Content = new StringContent(json, Encoding.UTF8, ContentTypes.ApplicationJson);

            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            if (!response.IsSuccessStatusCode)
            {
                return Result.Failure<string>($"HTTP request failed with status code: {response.StatusCode}");
            }

            await using var responseStream = await response.Content.ReadAsStreamAsync();
            var contentStream = response.Content.Headers.ContentEncoding.Contains(ContentEncodings.Gzip)
                ? new GZipStream(responseStream, CompressionMode.Decompress)
                : responseStream;

            using var reader = new StreamReader(contentStream, Encoding.UTF8);
            var result = await reader.ReadToEndAsync();

            if (contentStream is GZipStream gzipStream)
            {
                await gzipStream.DisposeAsync();
            }

            return Result.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in SendPostRequestAsync");
            return Result.Failure<string>($"Unexpected error", ex);
        }
    }

    private async Task<Result<string>> SendGetRequestAsync(string url, Dictionary<string, string> headers)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);

            foreach (var header in headers)
            {
                request.Headers.Add(header.Key, header.Value);
            }

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                return Result.Failure<string>($"HTTP GET request failed with status code: {response.StatusCode}");
            }

            var content = await response.Content.ReadAsStringAsync();
            return Result.Success(content);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request exception in SendGetRequestAsync");
            return Result.Failure<string>($"HTTP request failed: {ex.Message}");
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Request timeout in SendGetRequestAsync");
            return Result.Failure<string>("Request timeout");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in SendGetRequestAsync");
            return Result.Failure<string>($"Unexpected error: {ex.Message}");
        }
    }

    private void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _tokenRefreshTimer?.Dispose();

            // Securely clear encrypted token from memory
            if (_encryptedToken != null)
            {
                Array.Clear(_encryptedToken, 0, _encryptedToken.Length);
                _encryptedToken = null;
            }

            _disposed = true;
        }
    }

    #region Secure Token Handling

    private async Task SaveTokenSecurelyAsync(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return;
        }

        SecureString? secureToken = null;
        try
        {
            // Convert to SecureString for secure handling
            secureToken = EnvHelpers.CreateSecureString(token);
            await SaveTokenSecurelyAsync(secureToken);
        }
        finally
        {
            secureToken?.Dispose();
        }
    }

    private async Task SaveTokenSecurelyAsync(SecureString secureToken)
    {
        if (secureToken == null || secureToken.Length == 0)
        {
            return;
        }

        string? tempToken = null;
        try
        {
            // Briefly convert to string for encryption (minimizing exposure time)
            tempToken = EnvHelpers.SecureStringToString(secureToken);
            var tokenBytes = Encoding.UTF8.GetBytes(tempToken);
            var encryptedData = await EnvHelpers.EncryptTokenAesAsync(tokenBytes);
            await File.WriteAllBytesAsync(_tokenFilePath, encryptedData);

            // Also store in memory for quick access
            await StoreTokenSecurelyAsync(tempToken);
        }
        finally
        {
            if (tempToken != null)
            {
                EnvHelpers.SecureClearString(tempToken);
            }
        }
    }

    private async Task<Result<string>> LoadTokenSecurelyAsync()
    {
        if (!File.Exists(_tokenFilePath))
        {
            return Result.Failure<string>("Token file not found");
        }

        try
        {
            var encryptedData = await File.ReadAllBytesAsync(_tokenFilePath);
            var decryptedBytes = await EnvHelpers.DecryptTokenAesAsync(encryptedData);
            var token = Encoding.UTF8.GetString(decryptedBytes);

            if (string.IsNullOrEmpty(token))
            {
                return Result.Failure<string>("Token is null or empty after decryption");
            }

            return Result.Success(token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load token securely");
            return Result.Failure<string>($"Failed to load token securely",ex);
        }
    }

    private async Task StoreTokenSecurelyAsync(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            _logger.LogError("Token is null or empty");
            return;
        }

        var tokenBytes = Encoding.UTF8.GetBytes(token);
        _encryptedToken = await EnvHelpers.EncryptTokenAesAsync(tokenBytes);
    }

    private async Task<Result<string>> GetCurrentTokenAsync()
    {
        if (_encryptedToken == null)
        {
            return Result.Failure<string>("No token stored in memory");
        }

        try
        {
            var decryptedBytes = await EnvHelpers.DecryptTokenAesAsync(_encryptedToken);
            var token = Encoding.UTF8.GetString(decryptedBytes);
            await Task.CompletedTask; // For async consistency

            if (string.IsNullOrEmpty(token))
            {
                return Result.Failure<string>("Token is null or empty after decryption from memory");
            }

            return Result.Success(token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to decrypt token from memory");
            return Result.Failure<string>($"Failed to decrypt token from memory: {ex.Message}");
        }
    }

    #endregion

    /// <summary>
    /// Maps GitHub-specific device code response to application layer model
    /// </summary>
    /// <param name="githubResponse">The GitHub-specific response</param>
    /// <returns>Application layer device code response</returns>
    public static CopilotDeviceCodeResponse MapToApplicationModel(GithubDeviceCodeResponse githubResponse)
    {
        return new CopilotDeviceCodeResponse
        {
            DeviceCode = githubResponse.DeviceCode,
            UserCode = githubResponse.UserCode,
            VerificationUri = githubResponse.VerificationUri,
            ExpiresIn = githubResponse.ExpiresIn,
            Interval = githubResponse.Interval
        };
    }


}
