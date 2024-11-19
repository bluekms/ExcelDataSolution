using System.Globalization;
using System.Text;
using ExcelColumnExtractor.Containers;
using ExcelColumnExtractor.Exceptions;
using ExcelColumnExtractor.HeaderProcessors;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.Schemata;

namespace ExcelColumnExtractor.Checkers;

public static class DataBodyChecker
{
    public static void Check(
        IReadOnlyList<RawRecordSchema> recordSchemaList,
        RecordSchemaContainer recordSchemaContainer,
        ExtractedTableContainer extractedTableContainer,
        HeaderLengthContainer headerLengthContainer,
        ILogger<Program> logger)
    {
        var sb = new StringBuilder();
        foreach (var recordSchema in recordSchemaList)
        {
            try
            {
            }
            catch (Exception e)
            {
                sb.AppendLine(CultureInfo.InvariantCulture, $"{recordSchema}: {e.Message}");
                LogError(logger, recordSchema, e.Message, e);
            }
        }

        if (sb.Length > 0)
        {
            throw new DataBodyCheckerException(sb.ToString());
        }
    }

    private static readonly Action<ILogger, RawRecordSchema, string, Exception?> LogError =
        LoggerMessage.Define<RawRecordSchema, string>(LogLevel.Error, new EventId(0, nameof(DataBodyChecker)), "{RecordSchema}: {ErrorMessage}");
}
