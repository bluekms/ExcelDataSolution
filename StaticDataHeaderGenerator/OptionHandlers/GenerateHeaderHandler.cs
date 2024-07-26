using CLICommonLibrary;
using ExcelColumnExtractor.IniHandlers;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner.Schemata;
using StaticDataAttribute;
using StaticDataHeaderGenerator.ProgramOptions;

namespace StaticDataHeaderGenerator.OptionHandlers;

public static class GenerateHeaderHandler
{
    public static int Generate(GenerateHeaderOptions options)
    {
        var logger = string.IsNullOrEmpty(options.LogPath)
            ? Logger.CreateLoggerWithoutFile<Program>(options.MinLogLevel)
            : Logger.CreateLogger<Program>(options.MinLogLevel, options.LogPath);

        LogInformation(logger, "Generate Header File", null);

        var recordSchemaContainer = RecordScanner.Scan(options.RecordCsPath, logger);
        var recordSchemaList = recordSchemaContainer.RecordSchemaDictionary.Values
            .Where(x => x.HasAttribute<StaticDataRecordAttribute>())
            .Where(x => x.RecordName.Name == options.RecordName || x.RecordName.FullName.Contains(options.RecordName))
            .ToList();

        if (recordSchemaList.Count == 0)
        {
            var exception = new ArgumentException($"RecordName {options.RecordName} is not found.");
            LogError(logger, exception.Message, exception);
            throw exception;
        }
        else if (recordSchemaList.Count > 1)
        {
            LogWarning(logger, "Multiple records found with the specified name. Please provide a more specific name from the following options:", null);
            foreach (var recordSchema in recordSchemaList)
            {
                LogWarning(logger, $"\t{recordSchema.RecordName.FullName}", null);
            }

            return 0;
        }

        var targetRecordSchema = recordSchemaList.Single();
        var lengthRequiredNames = targetRecordSchema.FindLengthRequiredNames(recordSchemaContainer);
        var recordContainerInfo = new RecordContainerInfo(targetRecordSchema.RecordName, lengthRequiredNames);

        var results = IniReader.Read(options.LengthIniPath, new() { recordContainerInfo });

        // targetRecordSchema.Flatten(recordSchemaContainer, results.First().ToFrozenDictionary(), logger);

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
