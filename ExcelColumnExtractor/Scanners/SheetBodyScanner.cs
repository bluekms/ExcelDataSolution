using ExcelColumnExtractor.NameObjects;
using Microsoft.Extensions.Logging;

namespace ExcelColumnExtractor.Scanners;

public class SheetBodyScanner
{
    public sealed record RowData(IReadOnlyList<string> Data);
    public sealed record BodyData(IReadOnlyList<RowData> Rows);

    public static BodyData Scan(ExcelSheetName excelSheetName, ILogger logger)
    {
        List<RowData> rows = [];
        void ProcessBody(SheetBodyRow row)
        {
            rows.Add(new(row.Cells));
        }

        var processor = new ExcelSheetProcessor(ProcessBody);
        processor.Process(excelSheetName, logger);

        LogTrace(logger, excelSheetName.FullName, rows.Count, null);
        return new(rows.AsReadOnly());
    }

    private static readonly Action<ILogger, string, int, Exception?> LogTrace =
        LoggerMessage.Define<string, int>(
            LogLevel.Trace, new EventId(1, nameof(SheetBodyScanner)), "{ExcelSheetName}'s data is scanned. ({Count})");
}
