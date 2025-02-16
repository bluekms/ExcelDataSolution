using Microsoft.Extensions.Logging;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.Extensions;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.TypeCheckers;
using StaticDataAttribute;

namespace SchemaInfoScanner;

public static class RecordComplianceChecker
{
    public static void Check(RecordSchemaContainer recordSchemaContainer, ILogger logger)
    {
        var visited = new HashSet<RecordName>();

        if (recordSchemaContainer.StaticDataRecordSchemata.Count is 0)
        {
            throw new InvalidOperationException("No static data record is found.");
        }

        foreach (var recordSchema in recordSchemaContainer.StaticDataRecordSchemata)
        {
            if (!visited.Add(recordSchema.RecordName))
            {
                LogTrace(logger, $"{recordSchema.RecordName.FullName} is already visited.", null);
                continue;
            }

            foreach (var recordParameter in recordSchema.RawParameterSchemaList)
            {
                try
                {
                    SupportedTypeChecker.Check(recordParameter, recordSchemaContainer, visited, logger);
                }
                catch (Exception e)
                {
                    LogException(logger, $"{recordParameter.ParameterName.FullName}: {e.Message}", e);
                    throw;
                }
            }
        }
    }

    public static int TryCheck(RecordSchemaContainer recordSchemaContainer, ILogger logger)
    {
        var exceptionCount = 0;
        var visited = new HashSet<RecordName>();
        foreach (var recordSchema in recordSchemaContainer.StaticDataRecordSchemata)
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

            foreach (var recordParameter in recordSchema.RawParameterSchemaList)
            {
                try
                {
                    SupportedTypeChecker.Check(recordParameter, recordSchemaContainer, visited, logger);
                }
                catch (Exception e)
                {
                    exceptionCount += 1;
                    LogException(logger, $"{recordParameter.ParameterName.FullName}: {e.Message}", e);
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
