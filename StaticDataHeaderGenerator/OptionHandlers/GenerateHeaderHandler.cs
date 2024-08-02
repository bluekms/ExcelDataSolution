using CLICommonLibrary;
using ExcelColumnExtractor.IniHandlers;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner.Schemata;
using StaticDataAttribute;
using StaticDataHeaderGenerator.ProgramOptions;

namespace StaticDataHeaderGenerator.OptionHandlers;

public static class GenerateHeaderHandler
{
    private static readonly Action<ILogger, string, Exception?> LogTrace =
        LoggerMessage.Define<string>(LogLevel.Trace, new EventId(0, nameof(LogTrace)), "{Message}");

    private static readonly Action<ILogger, string, Exception?> LogInformation =
        LoggerMessage.Define<string>(LogLevel.Information, new EventId(0, nameof(LogInformation)), "{Message}");

    private static readonly Action<ILogger, string, Exception?> LogWarning =
        LoggerMessage.Define<string>(LogLevel.Warning, new EventId(0, nameof(LogWarning)), "{Message}");

    private static readonly Action<ILogger, string, Exception?> LogError =
        LoggerMessage.Define<string>(LogLevel.Error, new EventId(0, nameof(LogError)), "{Message}");

    public static int Generate(GenerateHeaderOptions options)
    {
        var logger = Logger.CreateLogger<Program>(options.MinLogLevel, options.LogPath);
        LogInformation(logger, "GenerateHeader", null);

        var lengthData = IniReader.Read(options.LengthIniPath);
        return 0;
    }
}
