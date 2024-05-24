using System.Collections.Immutable;
using System.Text.RegularExpressions;
using ExcelColumnExtractor.NameObjects;
using ExcelDataReader;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner.Exceptions;

namespace ExcelColumnExtractor.Scanners;

public static class HeaderScanner
{
    private static readonly Action<ILogger, string, Exception?> LogInformation =
        LoggerMessage.Define<string>(LogLevel.Information, new EventId(0, nameof(LogInformation)), "{Message}");

    public static ImmutableList<string> Scan(SheetName sheetName, ILogger logger)
    {
        using var loader = new LockedFileStreamLoader(sheetName.ExcelPath);
        if (loader.IsTemp)
        {
            var lastWriteTime = File.GetLastWriteTime(sheetName.ExcelPath);
            LogInformation(logger, $"{nameof(HeaderScanner)}: {Path.GetFileName(sheetName.ExcelPath)} 이미 열려있어 사본을 읽습니다. 마지막으로 저장된 시간: {lastWriteTime}", null);
        }

        using var reader = ExcelReaderFactory.CreateReader(loader.Stream);

        var sheetFound = false;
        do
        {
            if (reader.Name == sheetName.Name)
            {
                sheetFound = true;
                break;
            }
        }
        while (reader.NextResult());

        if (!sheetFound)
        {
            throw new ArgumentException($"{sheetName.Name} 을 찾을 수 없습니다. ExcelFile: {sheetName.ExcelPath}");
        }

        if (!reader.Read())
        {
            throw new EndOfStreamException($"{sheetName.Name} 예상치 못한 Sheet의 끝입니다.");
        }

        var startCell = reader.GetValue(0)?.ToString();
        if (startCell is null || !IsValidCellAddress(startCell))
        {
            throw new InvalidUsageException($"A1 셀에는 반드시 첫 번째 헤더의 Cell Address가 있어야 합니다. ex) B10");
        }

        var (startColumn, startRow) = ParseCellAddress(startCell);
        for (var rowIndex = 1; rowIndex < startRow; ++rowIndex)
        {
            if (!reader.Read())
            {
                throw new EndOfStreamException($"{sheetName.Name} 예상치 못한 Sheet의 끝입니다. A1: {startCell}");
            }
        }

        var headers = new List<string>();
        var duplicateChecker = new HashSet<string>();
        for (var colIndex = startColumn - 1; colIndex < reader.FieldCount; ++colIndex)
        {
            var header = reader.GetValue(colIndex)?.ToString();
            if (string.IsNullOrEmpty(header))
            {
                throw new EndOfStreamException($"{sheetName.Name} 예상치 못한 Header의 끝입니다. Last: {duplicateChecker.Last()}");
            }

            if (!duplicateChecker.Add(header))
            {
                throw new InvalidDataException($"{sheetName.Name} 중복된 Header가 있습니다. {header}");
            }

            headers.Add(header);
        }

        return headers.ToImmutableList();
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
}
