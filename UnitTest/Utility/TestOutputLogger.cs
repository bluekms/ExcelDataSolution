using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace UnitTest.Utility;

public class TestOutputLogger<T>(
    ITestOutputHelper output,
    LogLevel minLogLevel)
    : ILogger<T>
{
    public sealed record LogMessage(LogLevel LogLevel, string Message)
    {
        public override string ToString()
        {
            return $"[{LogLevel}]:\t{Message}";
        }
    }

    public List<LogMessage> Logs { get; } = new();

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        ArgumentNullException.ThrowIfNull(formatter);

        var message = formatter(state, exception);
        if (!string.IsNullOrEmpty(message))
        {
            var logMessage = new LogMessage(logLevel, message);
            output.WriteLine(logMessage.ToString());
            Logs.Add(logMessage);
        }
    }

    public bool IsEnabled(LogLevel logLevel) => logLevel >= minLogLevel;

    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull
    {
        return null;
    }
}
