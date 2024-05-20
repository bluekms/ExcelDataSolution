using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace UnitTest;

public class TestOutputLoggerFactory : ILoggerFactory
{
    private readonly ITestOutputHelper output;
    private readonly LogLevel minLogLevel;
    private bool disposed;

    public TestOutputLoggerFactory(ITestOutputHelper output, LogLevel minLogLevel)
    {
        this.output = output;
        this.minLogLevel = minLogLevel;
        this.disposed = false;
    }

    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
            }

            disposed = true;
        }
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new TestOutputLogger<object>(output, minLogLevel);
    }

    public ILogger<T> CreateLogger<T>()
    {
        return new TestOutputLogger<T>(output, minLogLevel);
    }

    public void AddProvider(ILoggerProvider provider)
    {
    }
}
