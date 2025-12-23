using System.Text;
using System.Text.RegularExpressions;
using CLICommonLibrary;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner;
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

        var catalogs = RecordScanner.Scan(options.RecordCsPath, logger);
        if (catalogs.RecordSchemaCatalog.StaticDataRecordSchemata.Count == 0)
        {
            var exception = new ArgumentException("No records found.");
            LogError(logger, exception.Message, exception);
            throw exception;
        }

        var sb = new StringBuilder();
        foreach (var targetRecordSchema in catalogs.RecordSchemaCatalog.StaticDataRecordSchemata)
        {
            var headers = RecordFlattener.Flatten(
                targetRecordSchema,
                catalogs.RecordSchemaCatalog,
                logger);

            var actualSeparator = Regex.Unescape(options.Separator);
            var output = $"[{targetRecordSchema.RecordName.FullName}]\n{string.Join(actualSeparator, headers)}\n";
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

    public static async Task<int> GenerateAsync(GenerateAllHeaderOptions options, CancellationToken cancellationToken = default)
    {
        var logger = string.IsNullOrEmpty(options.LogPath)
            ? Logger.CreateLoggerWithoutFile<Program>(options.MinLogLevel)
            : Logger.CreateLogger<Program>(options.MinLogLevel, options.LogPath);

        LogInformation(logger, "Generate Header File", null);

        var catalogs = await RecordScanner.ScanAsync(options.RecordCsPath, logger, cancellationToken);
        if (catalogs.RecordSchemaCatalog.StaticDataRecordSchemata.Count == 0)
        {
            var exception = new ArgumentException("No records found.");
            LogError(logger, exception.Message, exception);
            throw exception;
        }

        var sb = new StringBuilder();
        foreach (var targetRecordSchema in catalogs.RecordSchemaCatalog.StaticDataRecordSchemata)
        {
            var headers = RecordFlattener.Flatten(
                targetRecordSchema,
                catalogs.RecordSchemaCatalog,
                logger);

            var actualSeparator = Regex.Unescape(options.Separator);
            var output = $"[{targetRecordSchema.RecordName.FullName}]\n{string.Join(actualSeparator, headers)}\n";
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

            await File.WriteAllTextAsync(outputFileName, sb.ToString(), cancellationToken);

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
