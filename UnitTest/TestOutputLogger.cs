using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace UnitTest;

public class TestOutputLogger<T> : ILogger<T>
{
    private readonly ITestOutputHelper output;

    public TestOutputLogger(ITestOutputHelper output)
    {
        this.output = output;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (formatter is null)
        {
            throw new ArgumentNullException(nameof(formatter));
        }

        var message = formatter(state, exception);
        if (!string.IsNullOrEmpty(message))
        {
            this.output.WriteLine($"[{logLevel}] {message}");
        }
    }

    public bool IsEnabled(LogLevel logLevel) => true;

    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull
    {
        return null;
    }
}
