using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Display;

namespace CLICommonLibrary;

public static class Logger
{
    public static ILogger<T> CreateLogger<T>(LogEventLevel minLogLevel, string? logPath)
    {
        var outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}";
        var formatter = new MessageTemplateTextFormatter(outputTemplate, null);

        var loggerConfiguration = new LoggerConfiguration()
            .MinimumLevel.Is(minLogLevel)
            .WriteTo.Console(formatter);

        if (!string.IsNullOrEmpty(logPath))
        {
            loggerConfiguration.WriteTo.File(formatter, logPath, rollingInterval: RollingInterval.Day);
        }

        Log.Logger = loggerConfiguration.CreateLogger();

        var loggerFactory = new LoggerFactory().AddSerilog();

        return loggerFactory.CreateLogger<T>();
    }
}
