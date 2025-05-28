using Microsoft.Extensions.Logging;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.Extensions;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.TypeCheckers;
using StaticDataAttribute;

namespace SchemaInfoScanner;

public static class RecordComplianceChecker
{
    public static void Check(RecordSchemaCatalog recordSchemaCatalog, ILogger logger)
    {
        var visited = new HashSet<RecordName>();

        if (recordSchemaCatalog.StaticDataRecordSchemata.Count is 0)
        {
            throw new InvalidOperationException("No static data record is found.");
        }

        foreach (var recordSchema in recordSchemaCatalog.StaticDataRecordSchemata)
        {
            if (!visited.Add(recordSchema.RecordName))
            {
                LogTrace(logger, $"{recordSchema.RecordName.FullName} is already visited.", null);
                continue;
            }

            foreach (var recordParameter in recordSchema.RecordPropertySchemata)
            {
                try
                {
                    SupportedTypeChecker.Check(recordParameter, recordSchemaCatalog, visited, logger);
                }
                catch (Exception e)
                {
                    LogException(logger, $"{recordParameter.PropertyName.FullName}: {e.Message}", e);
                    throw;
                }
            }
        }
    }

    public static int TryCheck(RecordSchemaCatalog recordSchemaCatalog, ILogger logger)
    {
        var exceptionCount = 0;
        var visited = new HashSet<RecordName>();
        foreach (var recordSchema in recordSchemaCatalog.StaticDataRecordSchemata)
        {
            if (!recordSchema.HasAttribute<StaticDataRecordAttribute>())
            {
                continue;
            }

            if (!visited.Add(recordSchema.RecordName))
            {
                LogTrace(logger, $"{recordSchema.RecordName.FullName} is already visited.", null);
                continue;
            }

            foreach (var recordParameter in recordSchema.RecordPropertySchemata)
            {
                try
                {
                    SupportedTypeChecker.Check(recordParameter, recordSchemaCatalog, visited, logger);
                }
                catch (Exception e)
                {
                    exceptionCount += 1;
                    LogException(logger, $"{recordParameter.PropertyName.FullName}: {e.Message}", e);
                }
            }
        }

        return exceptionCount;
    }

    private static readonly Action<ILogger, string, Exception?> LogTrace =
        LoggerMessage.Define<string>(LogLevel.Trace, new EventId(0, nameof(LogTrace)), "{Message}");

    private static readonly Action<ILogger, string, Exception?> LogException =
        LoggerMessage.Define<string>(LogLevel.Error, new EventId(1, nameof(LogException)), "{Message}");
}
