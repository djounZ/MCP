using System.Text;
using System.Text.Json;

namespace MCP.Domain.Services.External
{
    public class CopilotService : ICopilotService, IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly Timer _tokenRefreshTimer;
        private string? _token;
        private bool _disposed;

        public CopilotService(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            
            // Start token refresh timer
            _tokenRefreshTimer = new Timer(async _ =>
            {
                await GetTokenAsync();
            }, null, TimeSpan.Zero, TimeSpan.FromMinutes(25));
        }

        public async Task SetupAsync()
        {
            var requestData = new
            {
                client_id = "Iv1.b507a08c87ecfe98",
                scope = "read:user"
            };

            var headers = new Dictionary<string, string>
            {
                ["accept"] = "application/json",
                ["editor-version"] = "Neovim/0.6.1",
                ["editor-plugin-version"] = "copilot.vim/1.16.0",
                ["content-type"] = "application/json",
                ["user-agent"] = "GithubCopilot/1.155.0",
                ["accept-encoding"] = "gzip,deflate,br"
            };

            var response = await SendPostRequestAsync("https://github.com/login/device/code", requestData, headers);
            using var responseJsonDoc = JsonDocument.Parse(response);
            var responseRoot = responseJsonDoc.RootElement;

            var deviceCode = responseRoot.GetProperty("device_code").GetString();
            var userCode = responseRoot.GetProperty("user_code").GetString();
            var verificationUri = responseRoot.GetProperty("verification_uri").GetString();

            Console.WriteLine($"Please visit {verificationUri} and enter code {userCode} to authenticate.");

            string? accessToken = null;
            while (accessToken == null)
            {
                await Task.Delay(5000);

                var tokenRequestData = new
                {
                    client_id = "Iv1.b507a08c87ecfe98",
                    device_code = deviceCode,
                    grant_type = "urn:ietf:params:oauth:grant-type:device_code"
                };

                var tokenResponse = await SendPostRequestAsync("https://github.com/login/oauth/access_token", tokenRequestData, headers);
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
                ["authorization"] = $"token {accessToken}",
                ["editor-version"] = "Neovim/0.6.1",
                ["editor-plugin-version"] = "copilot.vim/1.16.0",
                ["user-agent"] = "GithubCopilot/1.155.0"
            };

            var response = await SendGetRequestAsync("https://api.github.com/copilot_internal/v2/token", headers);
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
                ["authorization"] = $"Bearer {_token}"
            };

            try
            {
                var response = await SendPostRequestAsync("https://copilot-proxy.githubusercontent.com/v1/engines/copilot-codex/completions", requestData, headers);
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
                            var completion = choices[0].GetProperty("text").GetString();
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
            
            foreach (var header in headers)
            {
                if (header.Key == "content-type")
                    continue; // This will be set by StringContent
                request.Headers.Add(header.Key, header.Value);
            }

            var json = System.Text.Json.JsonSerializer.Serialize(data);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            return await response.Content.ReadAsStringAsync();
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
