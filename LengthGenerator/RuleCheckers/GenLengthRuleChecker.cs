using CLICommonLibrary;
using LengthGenerator.ProgramOptions;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner.Catalogs;
using SchemaInfoScanner.Extensions;
using SchemaInfoScanner.Schemata;
using StaticDataAttribute;

namespace LengthGenerator.RuleCheckers;

public static class GenLengthRuleChecker
{
    public sealed record Result(RecordSchema RecordSchema, RecordSchemaCatalog RecordSchemaCatalog);

    public static Result Check(GenLengthOptions options, ILogger logger)
    {
        var recordSchemaCatalog = RecordScanner.Scan(options.RecordCsPath, logger);
        if (recordSchemaCatalog.StaticDataRecordSchemata.Count == 0)
        {
            var exception = new ArgumentException($"No records found in the specified path: {options.RecordCsPath}");
            LogError(logger, exception.Message, exception);
            throw exception;
        }

        var candidates = recordSchemaCatalog.StaticDataRecordSchemata
            .Where(x => x.HasAttribute<StaticDataRecordAttribute>())
            .Where(x => x.RecordName.Name == options.RecordName);

        try
        {
            var recordSchema = candidates.SingleOrDefault();
            if (recordSchema is null)
            {
                var exception = new ArgumentException($"RecordName {options.RecordName} is not found.");
                LogError(logger, exception.Message, exception);
                throw exception;
            }

            return new(recordSchema, recordSchemaCatalog);
        }
        catch (Exception e)
        {
            LogError(logger, e.Message, e);
            throw;
        }
    }

    private static readonly Action<ILogger, string, Exception?> LogInformation =
        LoggerMessage.Define<string>(LogLevel.Information, new EventId(0, nameof(LogInformation)), "{Message}");

    private static readonly Action<ILogger, string, Exception?> LogWarning =
        LoggerMessage.Define<string>(LogLevel.Warning, new EventId(0, nameof(LogWarning)), "{Message}");

    private static readonly Action<ILogger, string, Exception?> LogError =
        LoggerMessage.Define<string>(LogLevel.Error, new EventId(0, nameof(LogError)), "{Message}");
}
