using CLICommonLibrary;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner;
using SchemaInfoScanner.Extensions;
using StaticDataHeaderGenerator.IniHandlers;
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

        var recordSchemaCatalog = RecordScanner.Scan(options.RecordCsPath, logger);
        if (recordSchemaCatalog.StaticDataRecordSchemata.Count == 0)
        {
            var exception = new ArgumentException($"RecordName {options.RecordName} is not found.");
            LogError(logger, exception.Message, exception);
            throw exception;
        }
        else if (recordSchemaCatalog.StaticDataRecordSchemata.Count > 1)
        {
            LogWarning(logger, "Multiple records found with the specified name. Please provide a more specific name from the following options:", null);
            foreach (var recordSchema in recordSchemaCatalog.StaticDataRecordSchemata)
            {
                LogWarning(logger, $"\t{recordSchema.RecordName.FullName}", null);
            }

            return 0;
        }

        var targetRecordSchema = recordSchemaCatalog.StaticDataRecordSchemata.Single();
        var lengthRequiredNames = LengthRequiringFieldDetector.Detect(
            targetRecordSchema,
            recordSchemaCatalog,
            logger);

        var recordContainerInfo = new RecordContainerInfo(targetRecordSchema.RecordName, lengthRequiredNames);

        var results = IniReader.Read(options.LengthIniPath, recordContainerInfo);
        var iniFileResult = results[targetRecordSchema.RecordName];
        var headers = RecordFlattener.Flatten(
            targetRecordSchema,
            recordSchemaCatalog,
            iniFileResult.HeaderNameLengths,
            logger);

        var output = $"[{targetRecordSchema.RecordName.FullName}]\n{string.Join(options.Separator, headers)}\n";
        LogInformation(logger, $"\n{output}\n", null);

        if (!string.IsNullOrEmpty(options.OutputFileName))
        {
            var outputFileName = string.IsNullOrEmpty(Path.GetExtension(options.OutputFileName))
                ? $"{options.OutputFileName}.txt"
                : options.OutputFileName;

            var directoryName = Path.GetDirectoryName(outputFileName);
            if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            File.WriteAllText(outputFileName, output);

            LogInformation(logger, $"Header file saved to {outputFileName}", null);
        }

        return 0;
    }

    private static readonly Action<ILogger, string, Exception?> LogInformation =
        LoggerMessage.Define<string>(LogLevel.Information, new EventId(0, nameof(LogInformation)), "{Message}");

    private static readonly Action<ILogger, string, Exception?> LogWarning =
        LoggerMessage.Define<string>(LogLevel.Warning, new EventId(0, nameof(LogWarning)), "{Message}");

    private static readonly Action<ILogger, string, Exception?> LogError =
        LoggerMessage.Define<string>(LogLevel.Error, new EventId(0, nameof(LogError)), "{Message}");
}
