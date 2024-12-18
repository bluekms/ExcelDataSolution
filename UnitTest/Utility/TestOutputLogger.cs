using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace UnitTest.Utility;

public class TestOutputLogger<T>(
    ITestOutputHelper output,
    LogLevel minLogLevel)
    : ILogger<T>
{
    public List<string> Logs { get; } = new();

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        ArgumentNullException.ThrowIfNull(formatter);

        var message = formatter(state, exception);
        if (!string.IsNullOrEmpty(message))
        {
            output.WriteLine($"[{logLevel}] {message}");
            Logs.Add(message);
        }
    }

    public bool IsEnabled(LogLevel logLevel) => logLevel >= minLogLevel;

    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull
    {
        return null;
    }
}
