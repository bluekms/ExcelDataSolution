using CLICommonLibrary;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner.Extensions;
using StaticDataAttribute;
using StaticDataHeaderGenerator.IniHandlers;
using StaticDataHeaderGenerator.ProgramOptions;

namespace StaticDataHeaderGenerator.OptionHandlers;

public static class GenerateLengthHandler
{
    public static int Generate(GenerateLengthOptions options)
    {
        var logger = string.IsNullOrEmpty(options.LogPath)
            ? Logger.CreateLoggerWithoutFile<Program>(options.MinLogLevel)
            : Logger.CreateLogger<Program>(options.MinLogLevel, options.LogPath);

        LogInformation(logger, "Generate Length Ini File", null);

        var recordSchemaContainer = RecordScanner.Scan(options.RecordCsPath, logger);
        var recordSchemaList = recordSchemaContainer.FindAll(options.RecordName);
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
        var lengthRequiredNames = LengthRequiringFieldDetector.Detect(
            targetRecordSchema,
            recordSchemaContainer,
            logger);

        var recordContainerInfo = new RecordContainerInfo(targetRecordSchema.RecordName, lengthRequiredNames);

        var writeResult = options.WriteMode switch
        {
            WriteModes.Overwrite => IniOverwriteWriter.Write(options.OutputPath, [recordContainerInfo]),
            WriteModes.Skip => IniSkipWriter.Write(options.OutputPath, [recordContainerInfo], logger),
            _ => throw new ArgumentOutOfRangeException(nameof(options), options.WriteMode, null)
        };

        LogInformation(logger, $"Generate is done. (WriteCount: {writeResult.WriteCount}, (Skip: {writeResult.SkipCount}))", null);

        return 0;
    }

    private static readonly Action<ILogger, string, Exception?> LogInformation =
        LoggerMessage.Define<string>(LogLevel.Information, new EventId(0, nameof(LogInformation)), "{Message}");

    private static readonly Action<ILogger, string, Exception?> LogWarning =
        LoggerMessage.Define<string>(LogLevel.Warning, new EventId(0, nameof(LogWarning)), "{Message}");

    private static readonly Action<ILogger, string, Exception?> LogError =
        LoggerMessage.Define<string>(LogLevel.Error, new EventId(0, nameof(LogError)), "{Message}");
}
