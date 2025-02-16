using CLICommonLibrary;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner.Extensions;
using StaticDataHeaderGenerator.IniHandlers;
using StaticDataHeaderGenerator.ProgramOptions;

namespace StaticDataHeaderGenerator.OptionHandlers;

public static class GenerateAllLengthHandler
{
    public static int Generate(GenerateAllLengthOptions options)
    {
        var logger = string.IsNullOrEmpty(options.LogPath)
            ? Logger.CreateLoggerWithoutFile<Program>(options.MinLogLevel)
            : Logger.CreateLogger<Program>(options.MinLogLevel, options.LogPath);

        LogInformation(logger, "Generate All Length Ini File", null);

        var recordSchemaContainer = RecordScanner.Scan(options.RecordCsPath, logger);

        if (recordSchemaContainer.StaticDataRecordSchemata.Count == 0)
        {
            var exception = new ArgumentException($"Record is not found.");
            LogError(logger, exception.Message, exception);
            throw exception;
        }

        var recordContainerInfos = new HashSet<RecordContainerInfo>();
        foreach (var targetRecordSchema in recordSchemaContainer.StaticDataRecordSchemata)
        {
            var lengthRequiredNames = LengthRequiringFieldDetector.Detect(
                targetRecordSchema,
                recordSchemaContainer,
                logger);

            if (lengthRequiredNames.Count == 0)
            {
                LogTrace(logger, $"{targetRecordSchema.RecordName.FullName} does not have any length required fields.", null);
                continue;
            }

            if (!recordContainerInfos.Add(new(targetRecordSchema.RecordName, lengthRequiredNames)))
            {
                LogInformation(logger, $"{targetRecordSchema.RecordName.FullName} is already added.", null);
                continue;
            }

            LogTrace(logger, $"{targetRecordSchema.RecordName.FullName} is added.", null);
        }

        LogTrace(logger, "Start writing ini files.", null);

        var writeResult = options.WriteMode switch
        {
            WriteModes.Overwrite => IniOverwriteWriter.Write(options.OutputPath, recordContainerInfos),
            WriteModes.Skip => IniSkipWriter.Write(options.OutputPath, recordContainerInfos, logger),
            _ => throw new ArgumentOutOfRangeException(nameof(options), options.WriteMode, null)
        };

        LogInformation(logger, $"Generate is done. (WriteCount: {writeResult.WriteCount}, (Skip: {writeResult.SkipCount}))", null);

        return 0;
    }

    private static readonly Action<ILogger, string, Exception?> LogTrace =
        LoggerMessage.Define<string>(LogLevel.Trace, new EventId(0, nameof(LogTrace)), "{Message}");

    private static readonly Action<ILogger, string, Exception?> LogInformation =
        LoggerMessage.Define<string>(LogLevel.Information, new EventId(0, nameof(LogInformation)), "{Message}");

    private static readonly Action<ILogger, string, Exception?> LogWarning =
        LoggerMessage.Define<string>(LogLevel.Warning, new EventId(0, nameof(LogWarning)), "{Message}");

    private static readonly Action<ILogger, string, Exception?> LogError =
        LoggerMessage.Define<string>(LogLevel.Error, new EventId(0, nameof(LogError)), "{Message}");
}
