﻿using Microsoft.Extensions.Logging;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.TypeCheckers;
using StaticDataAttribute;

namespace SchemaInfoScanner;

public static class Checker
{
    private static readonly Action<ILogger, string, Exception?> LogTrace =
        LoggerMessage.Define<string>(LogLevel.Trace, new EventId(0, nameof(LogTrace)), "{Message}");

    private static readonly Action<ILogger, string, Exception?> LogException =
        LoggerMessage.Define<string>(LogLevel.Error, new EventId(1, nameof(LogException)), "{Message}");

    public static void Check(RecordSchemaContainer recordSchemaContainer, SemanticModelContainer semanticModelContainer, ILogger logger)
    {
        var visited = new HashSet<RecordName>();
        foreach (var (_, recordSchema) in recordSchemaContainer.RecordSchemaDictionary)
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

            foreach (var recordParameter in recordSchema.RecordParameterSchemaList)
            {
                SupportedTypeChecker.Check(recordParameter, recordSchemaContainer, semanticModelContainer, visited, logger);
            }
        }
    }

    public static void TryCheck(RecordSchemaContainer recordSchemaContainer, SemanticModelContainer semanticModelContainer, ILogger logger)
    {
        var visited = new HashSet<RecordName>();
        foreach (var (_, recordSchema) in recordSchemaContainer.RecordSchemaDictionary)
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

            foreach (var recordParameter in recordSchema.RecordParameterSchemaList)
            {
                try
                {
                    SupportedTypeChecker.Check(recordParameter, recordSchemaContainer, semanticModelContainer, visited, logger);
                }
                catch (Exception e)
                {
                    LogException(logger, $"{recordParameter.ParameterName.FullName}: {e.Message}", e);
                }
            }
        }
    }
}
