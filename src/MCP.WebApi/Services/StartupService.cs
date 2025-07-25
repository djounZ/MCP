using System.Diagnostics;
using System.Runtime.InteropServices;
using MCP.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace MCP.WebApi.Services;

/// <summary>
/// Service that handles startup operations for the Web API
/// </summary>
public class StartupService
{
    private readonly ICopilotService _copilotService;
    private readonly ILogger<StartupService> _logger;

    public StartupService(ICopilotService copilotService, ILogger<StartupService> logger)
    {
        _copilotService = copilotService ?? throw new ArgumentNullException(nameof(copilotService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Initializes the application by registering device and opening verification URI
    /// </summary>
    /// <returns>Task representing the initialization operation</returns>
    public async Task InitializeAsync()
    {
        if (_copilotService.IsDeviceRegistered)
        {
            _logger.LogInformation("Device is already registered. Skipping device registration.");
            return;
        }

        _logger.LogInformation("Starting device registration for GitHub Copilot...");

        try
        {
            var registrationResult = await _copilotService.RegisterDeviceAsync();

            var success = await registrationResult.MatchAsync(
                onSuccess: async deviceResponse =>
                {
                    _logger.LogInformation("Device registration initiated successfully!");
                    _logger.LogInformation("User Code: {UserCode}", deviceResponse.UserCode);
                    _logger.LogInformation("Verification URI: {VerificationUri}", deviceResponse.VerificationUri);

                    // Create and open HTML authentication page
                    var htmlFilePath = await CreateAuthenticationPageAsync(deviceResponse.UserCode, deviceResponse.VerificationUri);
                    var browserProcess = await OpenBrowserAsync(htmlFilePath);

                    // Wait for the browser process to close
                    await WaitForBrowserCloseAsync(htmlFilePath, browserProcess);

                    _logger.LogInformation("Authentication completed. Starting the application...");
                    return true;
                },
                onFailure: async error =>
                {
                    _logger.LogError("Failed to register device: {Error}", error);
                    await Task.CompletedTask; // Make this async method actually async
                    throw new InvalidOperationException($"Device registration failed: {error}");
                });

            // This line will not be reached if there's a failure due to the exception
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during device registration");
            throw;
        }
    }

    /// <summary>
    /// Creates an HTML authentication page with instructions
    /// </summary>
    /// <param name="userCode">The device code to display</param>
    /// <param name="verificationUri">The GitHub verification URI</param>
    /// <returns>The path to the created HTML file</returns>
    private async Task<string> CreateAuthenticationPageAsync(string userCode, string verificationUri)
    {
        // Validate inputs and provide fallback values
        if (string.IsNullOrEmpty(userCode))
        {
            _logger.LogWarning("User code is empty. Using placeholder.");
            userCode = "USER-CODE-NOT-AVAILABLE";
        }

        if (string.IsNullOrEmpty(verificationUri))
        {
            _logger.LogWarning("Verification URI is empty. Using GitHub's default device login page.");
            verificationUri = "https://github.com/login/device";
        }

        var htmlContent = $@"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>GitHub Copilot Authentication</title>
    <style>
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
            max-width: 600px;
            margin: 0 auto;
            padding: 40px 20px;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            min-height: 100vh;
            color: #333;
        }}
        .container {{
            background: white;
            border-radius: 12px;
            padding: 40px;
            box-shadow: 0 20px 40px rgba(0,0,0,0.1);
            text-align: center;
        }}
        .logo {{
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
        }}
        h1 {{
            color: #24292e;
            margin-bottom: 10px;
            font-size: 28px;
        }}
        .subtitle {{
            color: #666;
            margin-bottom: 30px;
            font-size: 16px;
        }}
        .code-box {{
            background: #f6f8fa;
            border: 2px dashed #e1e4e8;
            border-radius: 8px;
            padding: 20px;
            margin: 30px 0;
            font-family: 'SF Mono', Monaco, 'Cascadia Code', monospace;
        }}
        .code {{
            font-size: 32px;
            font-weight: bold;
            color: #0366d6;
            letter-spacing: 4px;
        }}
        .instructions {{
            text-align: left;
            margin: 30px 0;
            padding: 20px;
            background: #f1f8ff;
            border-left: 4px solid #0366d6;
            border-radius: 4px;
        }}
        .step {{
            margin: 12px 0;
            display: flex;
            align-items: center;
        }}
        .step-number {{
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
        }}
        .btn {{
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
        }}
        .btn:hover {{
            background: #218838;
        }}
        .btn-secondary {{
            background: #6c757d;
        }}
        .btn-secondary:hover {{
            background: #5a6268;
        }}
        .footer {{
            margin-top: 40px;
            padding-top: 20px;
            border-top: 1px solid #e1e4e8;
            color: #666;
            font-size: 14px;
        }}
        .warning {{
            background: #fff3cd;
            border: 1px solid #ffeaa7;
            border-radius: 4px;
            padding: 15px;
            margin: 20px 0;
            color: #856404;
        }}
        @media (max-width: 480px) {{
            body {{ padding: 20px 10px; }}
            .container {{ padding: 20px; }}
            .code {{ font-size: 24px; letter-spacing: 2px; }}
        }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""logo"">GH</div>
        <h1>GitHub Copilot Authentication</h1>
        <p class=""subtitle"">Complete the authentication process to continue</p>
        
        <div class=""code-box"">
            <div>Enter this code:</div>
            <div class=""code"">{userCode}</div>
        </div>

        <div class=""instructions"">
            <div class=""step"">
                <div class=""step-number"">1</div>
                <div>Click the ""Continue to GitHub"" button below</div>
            </div>
            <div class=""step"">
                <div class=""step-number"">2</div>
                <div>Enter the code <strong>{userCode}</strong> on the GitHub page</div>
            </div>
            <div class=""step"">
                <div class=""step-number"">3</div>
                <div>Complete the authentication process</div>
            </div>
            <div class=""step"">
                <div class=""step-number"">4</div>
                <div>Close this browser tab/window when done</div>
            </div>
        </div>

        <div class=""warning"">
            <strong>⚠️ Important:</strong> The MCP application will start automatically once you close this browser window.
        </div>

        <a href=""{verificationUri}"" class=""btn"" target=""_blank"" rel=""noopener"">
            Continue to GitHub
        </a>
        
        <br>
        
        <button onclick=""window.close()"" class=""btn btn-secondary"">
            Close Window (After Authentication)
        </button>

        <div class=""footer"">
            <p>MCP - Model Context Protocol Server</p>
            <p>This window will automatically redirect you to GitHub for authentication.</p>
        </div>
    </div>

    <script>
        // Auto-focus and select the code for easy copying
        document.addEventListener('DOMContentLoaded', function() {{
            // Add click-to-copy functionality for the code
            document.querySelector('.code').addEventListener('click', function() {{
                navigator.clipboard.writeText('{userCode}').then(function() {{
                    const original = this.textContent;
                    this.textContent = 'Copied!';
                    setTimeout(() => {{
                        this.textContent = original;
                    }}, 1000);
                }}.bind(this));
            }});
            
            // Handle window closing
            let authenticationStarted = false;
            document.querySelector('a[href=""{verificationUri}""]').addEventListener('click', function() {{
                authenticationStarted = true;
            }});
            
            // Warn user if they try to close without authenticating
            window.addEventListener('beforeunload', function(e) {{
                if (!authenticationStarted) {{
                    e.preventDefault();
                    e.returnValue = 'Are you sure you want to close without authenticating?';
                }}
            }});
        }});
    </script>
</body>
</html>";

        var tempPath = Path.GetTempPath();
        var htmlFileName = $"mcp-copilot-auth-{DateTime.Now:yyyyMMdd-HHmmss}.html";
        var htmlFilePath = Path.Combine(tempPath, htmlFileName);

        await File.WriteAllTextAsync(htmlFilePath, htmlContent);
        _logger.LogInformation("Created authentication HTML page at: {FilePath}", htmlFilePath);

        return htmlFilePath;
    }

    /// <summary>
    /// Opens the HTML file in the default browser
    /// </summary>
    /// <param name="htmlFilePath">The path to the HTML file</param>
    /// <returns>The browser process that was started, or null if failed</returns>
    private async Task<Process?> OpenBrowserAsync(string htmlFilePath)
    {
        try
        {
            _logger.LogInformation("Opening authentication page...");
            
            return await Task.Run(() =>
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return Process.Start(new ProcessStartInfo
                    {
                        FileName = htmlFilePath,
                        UseShellExecute = true
                    });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    return Process.Start("open", htmlFilePath);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    return Process.Start("xdg-open", htmlFilePath);
                }
                else
                {
                    _logger.LogWarning("Unable to determine the operating system to open browser");
                    Console.WriteLine($"Please manually open: {htmlFilePath}");
                    return null;
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open authentication page: {FilePath}", htmlFilePath);
            Console.WriteLine($"Failed to open browser automatically. Please manually open: {htmlFilePath}");
            return null;
        }
        finally
        {
            _logger.LogInformation("Authentication page opened in browser");
        }
    }

    /// <summary>
    /// Waits for the browser process to close
    /// </summary>
    /// <param name="htmlFilePath">The path to the HTML file to clean up after</param>
    /// <param name="browserProcess">The browser process to monitor</param>
    private async Task WaitForBrowserCloseAsync(string htmlFilePath, Process? browserProcess)
    {
        _logger.LogInformation("Waiting for authentication to complete and browser to close...");

        if (browserProcess != null)
        {
            try
            {
                _logger.LogDebug("Monitoring browser process: {ProcessName} (ID: {ProcessId})", 
                    browserProcess.ProcessName, browserProcess.Id);

                // Wait for the specific browser process to exit
                await browserProcess.WaitForExitAsync();
                
                _logger.LogInformation("Browser process has closed. Authentication likely complete.");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error while waiting for browser process to close. Falling back to timeout.");
                
                // Fallback: wait for a reasonable amount of time if process monitoring fails
                await Task.Delay(30000); // 30 seconds fallback
            }
            finally
            {
                // Ensure the process is disposed
                try
                {
                    browserProcess?.Dispose();
                }
                catch
                {
                    // Ignore disposal errors
                }
            }
        }
        else
        {
            _logger.LogWarning("No browser process to monitor. Using fallback timeout.");
            
            // Fallback: wait for user input since we can't monitor the process
            Console.WriteLine("Press any key after you have completed the authentication and closed the browser...");
            await Task.Run(() => Console.ReadKey(true));
        }

        // Give time for authentication to complete before cleaning up
        _logger.LogInformation("Waiting a moment for authentication to complete...");
        await Task.Delay(5000); // Wait 5 seconds before cleanup

        // Clean up the temporary HTML file
        try
        {
            if (File.Exists(htmlFilePath))
            {
                File.Delete(htmlFilePath);
                _logger.LogDebug("Cleaned up temporary HTML file: {FilePath}", htmlFilePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to clean up temporary HTML file: {FilePath}", htmlFilePath);
        }
    }
}
