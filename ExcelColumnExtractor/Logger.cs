using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Formatting.Display;

namespace ExcelColumnExtractor;

public static class Logger
{
    public static ILogger<Program> CreateLogger(ProgramOptions options)
    {
        var outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}";
        var formatter = new MessageTemplateTextFormatter(outputTemplate, null);

        var loggerConfiguration = new LoggerConfiguration()
            .MinimumLevel.Is(options.MinLogLevel)
            .WriteTo.Console(formatter);

        if (!string.IsNullOrEmpty(options.LogPath))
        {
            loggerConfiguration.WriteTo.File(formatter, options.LogPath, rollingInterval: RollingInterval.Day);
        }

        Log.Logger = loggerConfiguration.CreateLogger();

        var loggerFactory = new LoggerFactory().AddSerilog();

        var logger = loggerFactory.CreateLogger<Program>();
        return logger;
    }
}
