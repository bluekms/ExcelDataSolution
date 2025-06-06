using System.Text;
using ExcelColumnExtractor.Containers;
using ExcelColumnExtractor.Exceptions;
using ExcelColumnExtractor.Scanners;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner.Schemata;

namespace ExcelColumnExtractor.Aggregator;

public static class BodyColumnAggregator
{
    public sealed record ExtractedRow(IReadOnlyList<string> Data);

    public sealed record ExtractedTable(IReadOnlyList<string> Headers, IReadOnlyList<ExtractedRow> Rows);

    public static ExtractedTableContainer Aggregate(
        IReadOnlyList<RecordSchema> recordSchemaList,
        ExcelSheetNameContainer sheetNameContainer,
        TargetColumnIndicesContainer targetColumnIndicesContainer,
        ILogger logger)
    {
        var result = new Dictionary<RecordSchema, ExtractedTable>();

        var sb = new StringBuilder();
        foreach (var recordSchema in recordSchemaList)
        {
            try
            {
                var excelSheetName = sheetNameContainer.Get(recordSchema);
                var sheetBody = SheetBodyScanner.Scan(excelSheetName, logger);
                var targetColumnData = targetColumnIndicesContainer.Get(recordSchema);

                var filteredRows = sheetBody.Rows
                    .Select(row => new ExtractedRow(row.Data
                        .Where((_, index) => targetColumnData.IndexSet.Contains(index))
                        .ToList()))
                    .ToList();

                result.Add(recordSchema, new(targetColumnData.Headers, filteredRows));
            }
            catch (Exception e)
            {
                sb.AppendLine(e.Message);
                LogError(logger, recordSchema, e.Message, e);
            }
        }

        return sb.Length > 0
            ? throw new BodyColumnAggregatorException(sb.ToString())
            : new(result);
    }

    private static readonly Action<ILogger, RecordSchema, string, Exception?> LogError =
        LoggerMessage.Define<RecordSchema, string>(LogLevel.Error, new EventId(0, nameof(LogError)), "{RecordSchema}: {ErrorMessage}");
}
