using System.Globalization;
using System.Text;
using ExcelColumnExtractor.Exceptions;
using ExcelColumnExtractor.Mappings;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner.Catalogs;
using SchemaInfoScanner.Schemata;

namespace ExcelColumnExtractor.Checkers;

public static class DataBodyChecker
{
    public static void Check(
        IReadOnlyList<RecordSchema> recordSchemaList,
        RecordSchemaCatalog recordSchemaCatalog,
        ExtractedTableMap extractedTableMap,
        ILogger<Program> logger)
    {
        var sb = new StringBuilder();

        // 스키마 순회
        foreach (var recordSchema in recordSchemaList)
        {
            try
            {
                // 프로퍼티 순회
                foreach (var propertySchema in recordSchema.PropertySchemata)
                {
                    // 레코드 순회?
                }
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
