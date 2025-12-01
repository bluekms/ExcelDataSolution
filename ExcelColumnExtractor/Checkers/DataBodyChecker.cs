using System.Globalization;
using System.Text;
using ExcelColumnExtractor.Containers;
using ExcelColumnExtractor.Exceptions;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner.Catalogs;
using SchemaInfoScanner.Schemata;

namespace ExcelColumnExtractor.Checkers;

public static class DataBodyChecker
{
    public static void Check(
        IReadOnlyList<RecordSchema> recordSchemaList,
        RecordSchemaCatalog recordSchemaCatalog,
        ExtractedTableContainer extractedTableContainer,
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

    private static readonly Action<ILogger, RecordSchema, string, Exception?> LogError =
        LoggerMessage.Define<RecordSchema, string>(LogLevel.Error, new EventId(0, nameof(DataBodyChecker)), "{RecordSchema}: {ErrorMessage}");
}
