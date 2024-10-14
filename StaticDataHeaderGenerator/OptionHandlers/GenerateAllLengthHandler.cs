using CLICommonLibrary;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner.Extensions;
using StaticDataAttribute;
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
        var recordSchemaList = recordSchemaContainer.RecordSchemaDictionary.Values
            .Where(x => x.HasAttribute<StaticDataRecordAttribute>())
            .OrderBy(x => x.RecordName.FullName)
            .ToList();

        if (recordSchemaList.Count == 0)
        {
            var exception = new ArgumentException($"Record is not found.");
            LogError(logger, exception.Message, exception);
            throw exception;
        }

        var recordContainerInfos = new HashSet<RecordContainerInfo>();
        foreach (var targetRecordSchema in recordSchemaList)
        {
            var lengthRequiredNames = targetRecordSchema.DetectLengthRequiringFields(recordSchemaContainer);
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

        IniWriter.Write(options.OutputPath, recordContainerInfos);
        LogInformation(logger, $"Generate is done. (Count: {recordContainerInfos.Count})", null);

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
