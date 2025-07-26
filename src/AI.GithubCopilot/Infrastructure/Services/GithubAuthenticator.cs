using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using AI.GithubCopilot.Infrastructure.Models;
using Microsoft.Extensions.Logging;

namespace AI.GithubCopilot.Infrastructure.Services;

public sealed class GithubAuthenticator(ILogger<GithubAuthenticator> logger, HttpListener httpListener, TaskCompletionSource<bool> authenticationCompleted)
{
    public async Task AuthenticateAsync(GithubDeviceCodeResponse deviceResponse, CancellationToken cancellationToken)
    {
        logger.LogInformation("Device registration initiated successfully!");
        logger.LogInformation("User Code: {UserCode}", deviceResponse.UserCode);
        logger.LogInformation("Verification URI: {VerificationUri}", deviceResponse.VerificationUri);
        var callbackPort = await StartCallbackServerAsync(cancellationToken);
        var htmlFilePath =
            await CreateAuthenticationPageAsync(deviceResponse.UserCode, deviceResponse.VerificationUri, callbackPort, cancellationToken);
        await OpenBrowserAsync(htmlFilePath, cancellationToken);
        await WaitForCloseButtonClickAsync(htmlFilePath, cancellationToken);

        logger.LogInformation("Authentication completed. Starting the application...");
    }

    private Task<int> StartCallbackServerAsync(CancellationToken cancellationToken)
    {
        var port = 8081; // Start with a default port
        const int maxAttempts = 10;

        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            try
            {
                var prefix = $"http://localhost:{port}/";
                httpListener.Prefixes.Add(prefix);
                httpListener.Start();

                logger.LogInformation("Callback server started on port {Port}", port);

                // Start handling requests in the background
                _ = Task.Run(() => HandleCallbackRequestsAsync(cancellationToken), cancellationToken);

                return Task.FromResult(port);
            }
            catch (HttpListenerException ex) when (ex.ErrorCode == 32) // Port already in use
            {
                httpListener.Prefixes.Clear();
                port++;
                logger.LogDebug("Port {Port} is in use, trying {NextPort}", port - 1, port);
            }
        }

        throw new InvalidOperationException($"Unable to start callback server after {maxAttempts} attempts");
    }

    private async Task HandleCallbackRequestsAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (httpListener.IsListening)
            {
                var context = await httpListener.GetContextAsync();
                var request = context.Request;
                var response = context.Response;

                // Set CORS headers to allow requests from the HTML page
                response.Headers.Add("Access-Control-Allow-Origin", "*");
                response.Headers.Add("Access-Control-Allow-Methods", "POST, OPTIONS");
                response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");

                if (string.Equals(request.HttpMethod, "Options", StringComparison.OrdinalIgnoreCase))
                {
                    // Handle preflight request
                    response.StatusCode = 200;
                    response.Close();
                    continue;
                }

                if (string.Equals(request.HttpMethod, "Post", StringComparison.OrdinalIgnoreCase) &&
                    request.Url?.AbsolutePath == "/auth-complete")
                {
                    logger.LogInformation("Received authentication completion signal from user");

                    // Send success response
                    var responseString = "{\"status\":\"success\"}";
                    var buffer = Encoding.UTF8.GetBytes(responseString);
                    response.ContentType = "application/json";
                    response.StatusCode = 200;
                    response.ContentLength64 = buffer.Length;
                    await response.OutputStream.WriteAsync(buffer, 0, buffer.Length, cancellationToken);
                    response.Close();

                    // Signal that authentication is complete
                    authenticationCompleted.TrySetResult(true);
                    break;
                }

                // Handle other requests
                response.StatusCode = 404;
                response.Close();
            }
        }
        catch (ObjectDisposedException)
        {
            // Listener was disposed, which is expected during shutdown
            logger.LogDebug("Callback server was disposed");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in callback server");
            authenticationCompleted.TrySetException(ex);
        }
    }

    private async Task<string> CreateAuthenticationPageAsync(string userCode, string verificationUri, int callbackPort,
        CancellationToken cancellationToken)
    {
        // Validate inputs and provide fallback values
        if (string.IsNullOrEmpty(userCode))
        {
            logger.LogWarning("User code is empty. Using placeholder.");
            userCode = "USER-CODE-NOT-AVAILABLE";
        }

        if (string.IsNullOrEmpty(verificationUri))
        {
            logger.LogWarning("Verification URI is empty. Using GitHub's default device login page.");
            verificationUri = "https://github.com/login/device";
        }

        var htmlContent = $$"""
                            <!DOCTYPE html>
                            <html lang="en">
                            <head>
                                <meta charset="UTF-8">
                                <meta name="viewport" content="width=device-width, initial-scale=1.0">
                                <title>GitHub Copilot Authentication</title>
                                <style>
                                    body {
                                        font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
                                        max-width: 600px;
                                        margin: 0 auto;
                                        padding: 40px 20px;
                                        background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                                        min-height: 100vh;
                                        color: #333;
                                    }
                                    .container {
                                        background: white;
                                        border-radius: 12px;
                                        padding: 40px;
                                        box-shadow: 0 20px 40px rgba(0,0,0,0.1);
                                        text-align: center;
                                    }
                                    .logo {
                                        width: 80px;
                                        height: 80px;
                                        margin: 0 auto 30px;
                                        background: #24292e;
                                        border-radius: 50%;
                                        display: flex;
                                        align-items: center;
                                        justify-content: center;
                                        color: white;
                                        font-size: 24px;
                                        font-weight: bold;
                                    }
                                    h1 {
                                        color: #24292e;
                                        margin-bottom: 10px;
                                        font-size: 28px;
                                    }
                                    .subtitle {
                                        color: #666;
                                        margin-bottom: 30px;
                                        font-size: 16px;
                                    }
                                    .code-box {
                                        background: #f6f8fa;
                                        border: 2px dashed #e1e4e8;
                                        border-radius: 8px;
                                        padding: 20px;
                                        margin: 30px 0;
                                        font-family: 'SF Mono', Monaco, 'Cascadia Code', monospace;
                                    }
                                    .code {
                                        font-size: 32px;
                                        font-weight: bold;
                                        color: #0366d6;
                                        letter-spacing: 4px;
                                    }
                                    .instructions {
                                        text-align: left;
                                        margin: 30px 0;
                                        padding: 20px;
                                        background: #f1f8ff;
                                        border-left: 4px solid #0366d6;
                                        border-radius: 4px;
                                    }
                                    .step {
                                        margin: 12px 0;
                                        display: flex;
                                        align-items: center;
                                    }
                                    .step-number {
                                        background: #0366d6;
                                        color: white;
                                        width: 24px;
                                        height: 24px;
                                        border-radius: 50%;
                                        display: flex;
                                        align-items: center;
                                        justify-content: center;
                                        margin-right: 12px;
                                        font-size: 12px;
                                        font-weight: bold;
                                        flex-shrink: 0;
                                    }
                                    .btn {
                                        background: #28a745;
                                        color: white;
                                        border: none;
                                        padding: 12px 30px;
                                        border-radius: 6px;
                                        font-size: 16px;
                                        font-weight: 600;
                                        cursor: pointer;
                                        text-decoration: none;
                                        display: inline-block;
                                        margin: 10px;
                                        transition: background 0.2s;
                                    }
                                    .btn:hover {
                                        background: #218838;
                                    }
                                    .btn-secondary {
                                        background: #6c757d;
                                    }
                                    .btn-secondary:hover {
                                        background: #5a6268;
                                    }
                                    .footer {
                                        margin-top: 40px;
                                        padding-top: 20px;
                                        border-top: 1px solid #e1e4e8;
                                        color: #666;
                                        font-size: 14px;
                                    }
                                    .warning {
                                        background: #fff3cd;
                                        border: 1px solid #ffeaa7;
                                        border-radius: 4px;
                                        padding: 15px;
                                        margin: 20px 0;
                                        color: #856404;
                                    }
                                    @media (max-width: 480px) {
                                        body { padding: 20px 10px; }
                                        .container { padding: 20px; }
                                        .code { font-size: 24px; letter-spacing: 2px; }
                                    }
                                </style>
                            </head>
                            <body>
                                <div class="container">
                                    <div class="logo">GH</div>
                                    <h1>GitHub Copilot Authentication</h1>
                                    <p class="subtitle">Complete the authentication process to continue</p>

                                    <div class="code-box">
                                        <div>Enter this code:</div>
                                        <div class="code">{{userCode}}</div>
                                    </div>

                                    <div class="instructions">
                                        <div class="step">
                                            <div class="step-number">1</div>
                                            <div>Click the "Continue to GitHub" button below</div>
                                        </div>
                                        <div class="step">
                                            <div class="step-number">2</div>
                                            <div>Enter the code <strong>{{userCode}}</strong> on the GitHub page</div>
                                        </div>
                                        <div class="step">
                                            <div class="step-number">3</div>
                                            <div>Complete the authentication process</div>
                                        </div>
                                        <div class="step">
                                            <div class="step-number">4</div>
                                            <div>Close this browser tab/window when done</div>
                                        </div>
                                    </div>

                                    <div class="warning">
                                        <strong>⚠️ Important:</strong> The MCP application will start automatically once you close this browser window.
                                    </div>

                                    <a href="{{verificationUri}}" class="btn" target="_blank" rel="noopener">
                                        Continue to GitHub
                                    </a>

                                    <br>

                                    <button onclick="closeWindow()" class="btn btn-secondary">
                                        Close Window (After Authentication)
                                    </button>

                                    <div class="footer">
                                        <p>MCP - Model Context Protocol Server</p>
                                        <p>This window will automatically redirect you to GitHub for authentication.</p>
                                    </div>
                                </div>

                                <script>
                                    // Auto-focus and select the code for easy copying
                                    document.addEventListener('DOMContentLoaded', function() {
                                        // Add click-to-copy functionality for the code
                                        document.querySelector('.code').addEventListener('click', function() {
                                            navigator.clipboard.writeText('{{userCode}}').then(function() {
                                                const original = this.textContent;
                                                this.textContent = 'Copied!';
                                                setTimeout(() => {
                                                    this.textContent = original;
                                                }, 1000);
                                            }.bind(this));
                                        });

                                        // Track authentication start
                                        let authenticationStarted = false;
                                        document.querySelector('a[href="{{verificationUri}}"]').addEventListener('click', function() {
                                            authenticationStarted = true;
                                        });

                                        // Warn user if they try to close without authenticating
                                        window.addEventListener('beforeunload', function(e) {
                                            if (!authenticationStarted) {
                                                e.preventDefault();
                                                e.returnValue = 'Are you sure you want to close without authenticating?';
                                            }
                                        });
                                    });

                                    // Function to handle close window button click
                                    function closeWindow() {
                                        // Call the callback endpoint to signal authentication completion
                                        fetch('http://localhost:{{callbackPort}}/auth-complete', {
                                            method: 'POST',
                                            headers: {
                                                'Content-Type': 'application/json'
                                            },
                                            body: JSON.stringify({ completed: true })
                                        })
                                        .then(() => {
                                            // Show confirmation message
                                            document.body.innerHTML = `
                                                <div style="text-align: center; padding: 50px; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;">
                                                    <h2 style="color: #28a745;">✅ Authentication Complete!</h2>
                                                    <p>The MCP application is starting now. You can close this tab.</p>
                                                    <button onclick="window.close()" style="background: #28a745; color: white; border: none; padding: 12px 30px; border-radius: 6px; font-size: 16px; cursor: pointer;">
                                                        Close Tab
                                                    </button>
                                                </div>
                                            `;

                                            // Auto-close after 3 seconds
                                            setTimeout(() => {
                                                window.close();
                                            }, 3000);
                                        })
                                        .catch(error => {
                                            console.error('Error signaling authentication completion:', error);
                                            // Still allow closing even if callback fails
                                            window.close();
                                        });
                                    }
                                </script>
                            </body>
                            </html>
                            """;

        var tempPath = Path.GetTempPath();
        var htmlFileName = $"mcp-copilot-auth-{DateTime.Now:yyyyMMdd-HHmmss}.html";
        var htmlFilePath = Path.Combine(tempPath, htmlFileName);

        await File.WriteAllTextAsync(htmlFilePath, htmlContent, cancellationToken);
        logger.LogInformation("Created authentication HTML page at: {FilePath}", htmlFilePath);

        return htmlFilePath;
    }

    private async Task OpenBrowserAsync(string htmlFilePath, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Opening authentication page...");

            await Task.Run(() =>
                {
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        Process.Start(new ProcessStartInfo { FileName = htmlFilePath, UseShellExecute = true });
                    }

                    if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    {
                        Process.Start("open", htmlFilePath);
                    }

                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    {
                        Process.Start("xdg-open", htmlFilePath);
                    }

                    logger.LogWarning("Unable to determine the operating system to open browser");
                },
                cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to open authentication page: {FilePath}", htmlFilePath);
        }
        finally
        {
            logger.LogInformation("Authentication page opened in browser");
        }
    }

    private async Task WaitForCloseButtonClickAsync(string htmlFilePath, CancellationToken cancellationToken)
    {
        logger.LogInformation("Waiting for user to complete authentication and click close button...");

        try
        {
            // Wait for the user to click the close button (which will trigger the callback)
            var timeoutTask = Task.Delay(TimeSpan.FromMinutes(10), cancellationToken); // 10 minute timeout
            var completedTask = await Task.WhenAny(authenticationCompleted.Task, timeoutTask);

            if (completedTask == timeoutTask)
            {
                logger.LogWarning("Authentication timed out after 10 minutes");
                throw new TimeoutException("Authentication process timed out");
            }

            var result = await authenticationCompleted.Task;
            if (result)
            {
                logger.LogInformation("User confirmed authentication completion");
            }
        }
        finally
        {
            // Stop the callback server
            try
            {
                httpListener.Stop();
                httpListener.Close();
                logger.LogDebug("Callback server stopped");
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Error stopping callback server");
            }

            // Clean up the temporary HTML file
            try
            {
                if (File.Exists(htmlFilePath))
                {
                    File.Delete(htmlFilePath);
                    logger.LogDebug("Cleaned up temporary HTML file: {FilePath}", htmlFilePath);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to clean up temporary HTML file: {FilePath}", htmlFilePath);
            }
        }
    }
}
