using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace UnitTest;

public class TestOutputLoggerFactory : ILoggerFactory
{
    private readonly ITestOutputHelper output;
    private bool disposed;

    public TestOutputLoggerFactory(ITestOutputHelper output)
    {
        this.output = output;
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
        return new TestOutputLogger<object>(output);
    }

    public ILogger<T> CreateLogger<T>()
    {
        return new TestOutputLogger<T>(output);
    }

    public void AddProvider(ILoggerProvider provider)
    {
    }
}
