using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace MCP.Infrastructure.Services;

public static class EnvHelpers
{


    public static async Task<T?> GetEnvironmentVariableAsync<T>(string environmentVariableName, ILogger logger)
    {

        try
        {
            var environmentVariable = Environment.GetEnvironmentVariable(environmentVariableName,EnvironmentVariableTarget.User);
            if (environmentVariable == null)
            {
                return default;
            }
            var decryptTokenAesStringAsync = await DecryptTokenAesStringAsync(environmentVariable);

            return JsonSerializer.Deserialize<T>(decryptTokenAesStringAsync);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to deserialize GitHub device code response from environment variable.");
            return default;
        }
    }




    public static async Task<bool> SetEnvironmentVariable<T>(string environmentVariableName, T value, ILogger logger)
    {

        try
        {
            var serialize = JsonSerializer.Serialize(value);
            var encryptTokenAesAsync = await EncryptTokenAesStringAsync(serialize);
            Environment.SetEnvironmentVariable(environmentVariableName, encryptTokenAesAsync,EnvironmentVariableTarget.User);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to deserialize GitHub device code response from environment variable.");
            return false;
        }
    }

    public static async Task<string> EncryptTokenAesStringAsync(string token)
    {
        var encryptTokenAesAsync = await EncryptTokenAesAsync(token);
        return Convert.ToBase64String(encryptTokenAesAsync);

    }

    private static async Task<byte[]> EncryptTokenAesAsync(string token)
    {
        var secureToken = CreateSecureString(token);
        if (secureToken.Length == 0)
        {
            return [];
        }

        string? tempToken = null;
        try
        {
            // Briefly convert to string for encryption (minimizing exposure time)
            tempToken = SecureStringToString(secureToken);
            var tokenBytes = Encoding.UTF8.GetBytes(tempToken);
            return await EncryptTokenAesAsync(tokenBytes);
        }
        finally
        {
            if (tempToken != null)
            {
                SecureClearString(tempToken);
            }
        }
    }

    /// <summary>
    /// Converts a SecureString back to a regular string (use sparingly and clear immediately after use)
    /// </summary>
    public static string SecureStringToString(SecureString secureString)
    {
        if (secureString.Length == 0)
        {
            return string.Empty;
        }

        var valuePtr = IntPtr.Zero;
        try
        {
            valuePtr = Marshal.SecureStringToGlobalAllocUnicode(secureString);
            return Marshal.PtrToStringUni(valuePtr) ?? string.Empty;
        }
        finally
        {
            if (valuePtr != IntPtr.Zero)
            {
                Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
            }
        }
    }

    public static void SecureClearString(string sensitiveString)
    {
        if (string.IsNullOrEmpty(sensitiveString))
        {
            return;
        }

        // Note: In .NET, strings are immutable, so we can't truly clear them from memory
        // The GC will eventually collect them, but we can't force immediate clearing
        // For truly sensitive data, use SecureString or byte arrays
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }

    /// <summary>
    /// Creates a SecureString from a regular string and then clears the original string
    /// </summary>
    public static SecureString CreateSecureString(string value)
    {
        var secureString = new SecureString();
        if (string.IsNullOrEmpty(value))
        {
            return secureString;
        }

        foreach (var c in value)
        {
            secureString.AppendChar(c);
        }
        secureString.MakeReadOnly();

        // Clear the original string from memory
        SecureClearString(value);

        return secureString;
    }

    // AES encryption for secure token storage
    public static async Task<byte[]> EncryptTokenAesAsync(byte[] data)
    {
        var keyMaterial = Environment.MachineName + Environment.UserName + "CopilotServiceKey2025";
        var key = SHA256.HashData(Encoding.UTF8.GetBytes(keyMaterial))[..32];

        using var aes = Aes.Create();
        aes.Key = key;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        using var msEncrypt = new MemoryStream();

        // Write IV first
        await msEncrypt.WriteAsync(aes.IV, 0, aes.IV.Length);

        await using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
        {
            await csEncrypt.WriteAsync(data, 0, data.Length);
        }

        return msEncrypt.ToArray();
    }


    public static async Task<string> DecryptTokenAesStringAsync(string envValue)
    {

        var decryptTokenAesAsync = await DecryptTokenAesAsync(envValue);
        return Encoding.UTF8.GetString(decryptTokenAesAsync);
    }

    private static async Task<byte[]> DecryptTokenAesAsync(string envValue)
    {
        var byteArray = Convert.FromBase64String(envValue);
        return await DecryptTokenAesAsync(byteArray);
    }

    public static async Task<byte[]> DecryptTokenAesAsync(byte[] encryptedData)
    {
        var keyMaterial = Environment.MachineName + Environment.UserName + "CopilotServiceKey2025";
        var key = SHA256.HashData(Encoding.UTF8.GetBytes(keyMaterial))[..32];

        using var aes = Aes.Create();
        aes.Key = key;

        // Extract IV from the beginning of the encrypted data
        var iv = new byte[aes.BlockSize / 8];
        Array.Copy(encryptedData, 0, iv, 0, iv.Length);
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor();
        using var msDecrypt = new MemoryStream(encryptedData, iv.Length, encryptedData.Length - iv.Length);
        await using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
        using var msPlain = new MemoryStream();

        await csDecrypt.CopyToAsync(msPlain);
        return msPlain.ToArray();
    }
}
