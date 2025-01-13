using System.Text;
using CLICommonLibrary;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner;
using SchemaInfoScanner.Extensions;
using StaticDataAttribute;
using StaticDataHeaderGenerator.IniHandlers;
using StaticDataHeaderGenerator.ProgramOptions;

namespace StaticDataHeaderGenerator.OptionHandlers;

public class GenerateAllHeaderHandler
{
    public static int Generate(GenerateAllHeaderOptions options)
    {
        var logger = string.IsNullOrEmpty(options.LogPath)
            ? Logger.CreateLoggerWithoutFile<Program>(options.MinLogLevel)
            : Logger.CreateLogger<Program>(options.MinLogLevel, options.LogPath);

        LogInformation(logger, "Generate Header File", null);

        var recordSchemaContainer = RecordScanner.Scan(options.RecordCsPath, logger);
        if (recordSchemaContainer.StaticDataRecordSchemata.Count == 0)
        {
            var exception = new ArgumentException("No records found.");
            LogError(logger, exception.Message, exception);
            throw exception;
        }

        var sb = new StringBuilder();
        foreach (var targetRecordSchema in recordSchemaContainer.StaticDataRecordSchemata)
        {
            var lengthRequiredNames = LengthRequiringFieldDetector.Detect(
                targetRecordSchema,
                recordSchemaContainer,
                logger);

            var recordContainerInfo = new RecordContainerInfo(targetRecordSchema.RecordName, lengthRequiredNames);
            if (recordContainerInfo.LengthRequiredHeaderNames.Count == 0)
            {
                LogTrace(logger, $"No length required header names found for {targetRecordSchema.RecordName.FullName}", null);
                continue;
            }

            var results = IniReader.Read(options.LengthIniPath, recordContainerInfo);
            var iniFileResult = results[targetRecordSchema.RecordName];
            var headers = RecordFlattener.Flatten(
                targetRecordSchema,
                recordSchemaContainer,
                iniFileResult.HeaderNameLengths,
                logger);

            var output = $"[{targetRecordSchema.RecordName.FullName}]\n{string.Join(options.Separator, headers)}\n";
            sb.AppendLine(output);

            LogInformation(logger, $"\n{output}\n", null);
        }

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

            File.WriteAllText(outputFileName, sb.ToString());

            LogInformation(logger, $"Header file saved to {outputFileName}", null);
        }

        return 0;
    }

    private static readonly Action<ILogger, string, Exception?> LogTrace =
        LoggerMessage.Define<string>(LogLevel.Trace, new EventId(0, nameof(LogTrace)), "{Message}");

    private static readonly Action<ILogger, string, Exception?> LogInformation =
        LoggerMessage.Define<string>(LogLevel.Information, new EventId(0, nameof(LogInformation)), "{Message}");

    private static readonly Action<ILogger, string, Exception?> LogError =
        LoggerMessage.Define<string>(LogLevel.Error, new EventId(0, nameof(LogError)), "{Message}");
}
