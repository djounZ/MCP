using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace AI.GithubCopilot.Tests.Integration;

public class TestOutputLoggerProvider(ITestOutputHelper output) : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName)
    {
        return new TestOutputLogger(output, categoryName);
    }

    public void Dispose()
    {
        // No resources to dispose
    }

    private class TestOutputLogger(ITestOutputHelper output, string categoryName) : ILogger
    {
        private static readonly IDisposable DisposableMock = new NoOpDisposable();

        IDisposable ILogger.BeginScope<TState>(TState state)
        {
            return DisposableMock;
        }

        private class NoOpDisposable : IDisposable { public void Dispose() { } }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            try
            {
                output.WriteLine($"[{logLevel}] {categoryName}: {formatter(state, exception)}");
                if (exception != null)
                {
                    output.WriteLine(exception.ToString());
                }
            }
            catch
            {
                 /* Ignore output errors */
            }
        }
    }
}
