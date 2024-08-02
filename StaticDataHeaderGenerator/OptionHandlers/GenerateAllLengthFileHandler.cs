using CLICommonLibrary;
using ExcelColumnExtractor.IniHandlers;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner.Schemata;
using StaticDataAttribute;
using StaticDataHeaderGenerator.ProgramOptions;

namespace StaticDataHeaderGenerator.OptionHandlers;

public static class GenerateAllLengthFileHandler
{
    private static readonly Action<ILogger, string, Exception?> LogTrace =
        LoggerMessage.Define<string>(LogLevel.Trace, new EventId(0, nameof(LogTrace)), "{Message}");

    private static readonly Action<ILogger, string, Exception?> LogInformation =
        LoggerMessage.Define<string>(LogLevel.Information, new EventId(0, nameof(LogInformation)), "{Message}");

    private static readonly Action<ILogger, string, Exception?> LogWarning =
        LoggerMessage.Define<string>(LogLevel.Warning, new EventId(0, nameof(LogWarning)), "{Message}");

    private static readonly Action<ILogger, string, Exception?> LogError =
        LoggerMessage.Define<string>(LogLevel.Error, new EventId(0, nameof(LogError)), "{Message}");

    public static int Generate(GenerateAllLengthTemplateOptions options)
    {
        var logger = Logger.CreateLogger<Program>(options.MinLogLevel, options.LogPath);
        LogInformation(logger, "GenerateLengthFile", null);

        var recordSchemaContainer = RecordScanner.Scan(options.RecordCsPath, logger);
        var recordContainerInfos = new HashSet<RecordContainerInfo>();
        foreach (var recordSchema in recordSchemaContainer.RecordSchemaDictionary.Values.OrderBy(x => x.RecordName.FullName))
        {
            if (!recordSchema.HasAttribute<StaticDataRecordAttribute>())
            {
                LogTrace(logger, $"{recordSchema.RecordName.FullName} is skip. It's not StaticDataRecord.", null);
                continue;
            }

            var lengthRequiredNames = recordSchema.FindLengthRequiredNames(recordSchemaContainer);
            recordContainerInfos.Add(new(recordSchema.RecordName.FullName, lengthRequiredNames));

            LogTrace(logger, $"{recordSchema.RecordName.FullName} is added.", null);
        }

        IniWriter.Write(options.OutputPath, recordContainerInfos);
        LogInformation(logger, "GenerateAllLengthFileHandler is done.", null);

        return 0;
    }

}
