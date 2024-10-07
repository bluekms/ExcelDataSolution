using System.Text.RegularExpressions;
using ExcelColumnExtractor.NameObjects;
using ExcelDataReader;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner.Exceptions;

namespace ExcelColumnExtractor.Scanners;

public sealed class ExcelSheetProcessor
{
    public delegate void ProcessHeader(SheetHeader header);
    public delegate void ProcessBodyRow(SheetBodyRow row);

    private readonly ProcessHeader? processHeader;
    private readonly ProcessBodyRow? processBodyRow;

    public ExcelSheetProcessor(
        ProcessHeader processHeader,
        ProcessBodyRow processBodyRow)
    {
        this.processHeader = processHeader;
        this.processBodyRow = processBodyRow;
    }

    public ExcelSheetProcessor(ProcessHeader processHeader)
    {
        this.processHeader = processHeader;
        this.processBodyRow = null;
    }

    public ExcelSheetProcessor(ProcessBodyRow processBodyRow)
    {
        this.processHeader = null;
        this.processBodyRow = processBodyRow;
    }

    public void Process(ExcelSheetName excelSheetName, ILogger logger)
    {
        using var loader = new LockedFileStreamOpener(excelSheetName.ExcelPath);
        if (loader.IsTemp)
        {
            var lastWriteTime = File.GetLastWriteTime(excelSheetName.ExcelPath);
            LogInformation(logger, $"{nameof(SheetHeaderScanner)}: {Path.GetFileName(excelSheetName.ExcelPath)} 이미 열려있어 사본을 읽습니다. 마지막으로 저장된 시간: {lastWriteTime}", null);
        }

        using var reader = ExcelReaderFactory.CreateReader(loader.Stream);

        var sheetFound = false;
        do
        {
            if (reader.Name == excelSheetName.SheetName)
            {
                sheetFound = true;
                break;
            }
        }
        while (reader.NextResult());

        if (!sheetFound)
        {
            throw new ArgumentException($"{excelSheetName.FullName} 을 찾을 수 없습니다.");
        }

        if (!reader.Read())
        {
            throw new EndOfStreamException($"{excelSheetName.FullName} 예상치 못한 Sheet의 끝입니다.");
        }

        var startCell = reader.GetValue(0)?.ToString();
        if (startCell is null || !IsValidCellAddress(startCell))
        {
            throw new InvalidUsageException($"A1 셀에는 반드시 첫 번째 헤더의 Cell Address가 있어야 합니다. ex) B10");
        }

        var (startColumn, startRow) = ParseCellAddress(startCell);
        while (--startRow > 0)
        {
            if (!reader.Read())
            {
                throw new EndOfStreamException($"{excelSheetName.FullName} 예상치 못한 Sheet의 끝입니다. A1: {startCell}");
            }
        }

        if (processHeader is not null)
        {
            var cells = new object[reader.FieldCount];
            reader.GetValues(cells);

            var headerCells = cells
                .Skip(startColumn - 1)
                .Select(x => x?.ToString() ?? string.Empty)
                .ToList();

            processHeader(new(headerCells));
        }
        else
        {
            reader.Read();
        }

        if (processBodyRow is null)
        {
            return;
        }

        while (reader.Read())
        {
            var cells = new object[reader.FieldCount];
            reader.GetValues(cells);

            var rowCells = cells
                .Skip(startColumn - 1)
                .Select(x => x?.ToString() ?? string.Empty)
                .ToList();

            processBodyRow(new(rowCells));
        }
    }

    private static bool IsValidCellAddress(string cellAddress)
    {
        var regex = new Regex(@"^[A-Z]+[0-9]+$", RegexOptions.IgnoreCase);
        return regex.IsMatch(cellAddress);
    }

    private static (int Column, int Row) ParseCellAddress(string cellAddress)
    {
        var row = 0;
        var column = 0;

        foreach (var c in cellAddress)
        {
            if (char.IsDigit(c))
            {
                row = (row * 10) + (c - '0');
            }
            else if (char.IsLetter(c))
            {
                column = (column * 26) + (c - 'A' + 1);
            }
        }

        return (column, row);
    }

    private static readonly Action<ILogger, string, Exception?> LogInformation =
        LoggerMessage.Define<string>(LogLevel.Information, new EventId(0, nameof(LogInformation)), "{Message}");
}
