using CLICommonLibrary;
using LengthGenerator.ProgramOptions;
using LengthGenerator.RuleCheckers;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner.Extensions;

namespace LengthGenerator.OptionHandlers;

public static class GenLengthHandler
{
    public static int Generate(GenLengthOptions options)
    {
        var logger = string.IsNullOrEmpty(options.LogPath)
            ? Logger.CreateLoggerWithoutFile<Program>(options.MinLogLevel)
            : Logger.CreateLogger<Program>(options.MinLogLevel, options.LogPath);

        LogInformation(logger, "Generate Length File", null);

        var result = GenLengthRuleChecker.Check(options, logger);

        var lengthRequiredNames = LengthRequiringFieldDetector.Detect(
            result.RecordSchema,
            result.RecordSchemaCatalog,
            logger);


        return 0;
    }

    private static readonly Action<ILogger, string, Exception?> LogInformation =
        LoggerMessage.Define<string>(LogLevel.Information, new EventId(0, nameof(LogInformation)), "{Message}");

    private static readonly Action<ILogger, string, Exception?> LogWarning =
        LoggerMessage.Define<string>(LogLevel.Warning, new EventId(0, nameof(LogWarning)), "{Message}");

    private static readonly Action<ILogger, string, Exception?> LogError =
        LoggerMessage.Define<string>(LogLevel.Error, new EventId(0, nameof(LogError)), "{Message}");
}
