using System.Collections.Immutable;
using ExcelColumnExtractor.Scanners;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.Schemata;

namespace ExcelColumnExtractor.Checkers;

public static class HeaderChecker
{
    private static readonly Action<ILogger, string, Exception?> LogTrace =
        LoggerMessage.Define<string>(LogLevel.Trace, new EventId(0, nameof(LogTrace)), "{Message}");

    private record ColumnPatternInfo(RecordParameterName ParameterName, ImmutableList<string> Patterns);

    public static void Check(
        ImmutableList<string> headerList,
        RecordSchema recordSchema,
        RecordSchemaContainer recordSchemaContainer,
        ILogger logger)
    {
        var columnPatternInfos = new Dictionary<RecordParameterName, ColumnPatternInfo>();
        foreach (var recordParameterSchema in recordSchema.RecordParameterSchemaList)
        {
            var patterns = ColumnPatternGenerator.Generate(recordParameterSchema, recordSchemaContainer, logger);
            columnPatternInfos[recordParameterSchema.ParameterName] = new ColumnPatternInfo(recordParameterSchema.ParameterName, patterns);
        }

        foreach (var (_, info) in columnPatternInfos)
        {
            // var regex = new Regex(info.Patterns);
            // var matchCount = headerList.Count(header => regex.IsMatch(header));
            //
            // LogTrace(logger, $"{pattern}: {matchCount}", null);
        }
    }
}
