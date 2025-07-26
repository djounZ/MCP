using System.Text.Encodings.Web;
using System.Text.Json;
using MCP.Infrastructure.Models;
using MCP.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit.Abstractions;

namespace MCP.Infrastructure.Tests
{
    public class EnvHelpersTests(ITestOutputHelper testOutputHelper) : IDisposable
    {
        private readonly ILogger _logger = NullLogger.Instance;
        private readonly List<string> _createdEnvironmentVariables = new();

        [Fact]
        public async Task SetEnvironmentVariable_ShouldSetAndGetValue()
        {
            var key = "TestEnvVar";
            _createdEnvironmentVariables.Add(key);
            var value = new TestData { Name = "Test", Number = 42 };

            var setResult = await EnvHelpers.SetEnvironmentVariable(key, value, _logger);
            Assert.True(setResult);

            var getResult = await EnvHelpers.GetEnvironmentVariableAsync<TestData>(key, _logger);
            Assert.NotNull(getResult);
            Assert.Equal(value.Name, getResult.Name);
            Assert.Equal(value.Number, getResult.Number);
        }

        [Fact]
        public async Task GetEnvironmentVariableAsync_ShouldReturnDefault_WhenNotSet()
        {
            var key = "NonExistentEnvVar";
            var result = await EnvHelpers.GetEnvironmentVariableAsync<TestData>(key, _logger);
            Assert.Null(result);
        }

        [Fact]
        public void CreateSecureString_ShouldReturnSecureString()
        {
            var input = "SensitiveData";
            var secure = EnvHelpers.CreateSecureString(input);
            Assert.Equal(input.Length, secure.Length);
            Assert.True(secure.IsReadOnly());
        }

        [Fact]
        public async Task SetEnvironmentVariable_WithComplexObject_ShouldSetAndGetCorrectly()
        {
            var key = "TestComplexEnvVar";
            _createdEnvironmentVariables.Add(key);
            var value = new TestData
            {
                Name = "Complex Test Data with Special Characters: !@#$%^&*()",
                Number = -42
            };

            var setResult = await EnvHelpers.SetEnvironmentVariable(key, value, _logger);
            Assert.True(setResult);

            var getResult = await EnvHelpers.GetEnvironmentVariableAsync<TestData>(key, _logger);
            Assert.NotNull(getResult);
            Assert.Equal(value.Name, getResult.Name);
            Assert.Equal(value.Number, getResult.Number);
        }

        [Fact]
        public async Task EnvironmentVariable_AfterCleanup_ShouldNotExist()
        {
            var key = "TestCleanupEnvVar";
            var value = new TestData { Name = "Temporary", Number = 123 };

            // Set the environment variable
            await EnvHelpers.SetEnvironmentVariable(key, value, _logger);

            // Verify it exists
            var getResult = await EnvHelpers.GetEnvironmentVariableAsync<TestData>(key, _logger);
            Assert.NotNull(getResult);

            // Clean it up manually
            Environment.SetEnvironmentVariable(key, null, EnvironmentVariableTarget.User);

            // Verify it's gone
            var cleanupResult = await EnvHelpers.GetEnvironmentVariableAsync<TestData>(key, _logger);
            Assert.Null(cleanupResult);
        }

        [Fact]
        public async Task SetEnvironmentVariable_WithQuotes_ShouldPreserveQuotes()
        {
            var key = "TestQuotesEnvVar";
            _createdEnvironmentVariables.Add(key);
            var value = new TestData
            {
                Name = "Test with \"quotes\" and 'apostrophes'",
                Number = 42
            };

            var setResult = await EnvHelpers.SetEnvironmentVariable(key, value, _logger);
            Assert.True(setResult);

            var getResult = await EnvHelpers.GetEnvironmentVariableAsync<TestData>(key, _logger);
            Assert.NotNull(getResult);
            Assert.Equal(value.Name, getResult.Name);
            Assert.Equal(value.Number, getResult.Number);

            // Specifically test that quotes are preserved
            Assert.Contains("\"quotes\"", getResult.Name);
            Assert.Contains("'apostrophes'", getResult.Name);
            Assert.DoesNotContain("\\u0022", getResult.Name);
        }

        [Fact]
        public void JsonSerialization_WithQuotes_ShouldPreserveQuotes()
        {
            // Test the JSON serialization directly to identify the issue
            var testData = new TestData
            {
                Name = "Test with \"quotes\" and 'apostrophes'",
                Number = 42
            };

            // Test with the same options that EnvHelpers uses
            var jsonOptions = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            var serialized = JsonSerializer.Serialize(testData, jsonOptions);
            // Output the serialized JSON to see what it contains
            Assert.NotNull(serialized);

            var deserialized = JsonSerializer.Deserialize<TestData>(serialized, jsonOptions);

            Assert.NotNull(deserialized);
            Assert.Equal(testData.Name, deserialized.Name);
            Assert.Contains("\"quotes\"", deserialized.Name);
            Assert.DoesNotContain("\\u0022", deserialized.Name);

            // Additional debug info - check if serialized JSON contains unicode escapes
            Assert.DoesNotContain("\\u0022", serialized);
        }

        [Fact]
        public async Task DecryptSpecificEncryptedValue_ShouldWork()
        {
            var githubAccessTokenResponse = new GithubAccessTokenResponse
            {
                AccessToken = "toto",
                TokenType = "bearer"
            };


            var serialize = JsonSerializer.Serialize(githubAccessTokenResponse);
            var encryptedValue = await EnvHelpers.EncryptTokenAesStringAsync(serialize);
            try
            {
                // Directly decrypt the string
                var decryptedJson = await EnvHelpers.DecryptTokenAesStringAsync(encryptedValue);
                Assert.NotNull(decryptedJson);
                Assert.NotEmpty(decryptedJson);

                // Output the decrypted JSON to see what it contains
                testOutputHelper.WriteLine($"Decrypted JSON: {decryptedJson}");

                // Second deserialization gets us the actual object
                var result = JsonSerializer.Deserialize<GithubAccessTokenResponse>(decryptedJson);
                Assert.Equal(result,githubAccessTokenResponse);

            }
            catch (Exception ex)
            {
                // If it fails, output the exception for debugging
                Assert.Fail($"Decryption failed: {ex.Message}");
            }
        }

        public void Dispose()
        {
            // Clean up all environment variables created during tests
            foreach (var key in _createdEnvironmentVariables)
            {
                Environment.SetEnvironmentVariable(key, null, EnvironmentVariableTarget.User);
            }
            _createdEnvironmentVariables.Clear();
        }

        private class TestData
        {
            public string Name { get; init; } = string.Empty;
            public int Number { get; init; }
        }
    }
}
