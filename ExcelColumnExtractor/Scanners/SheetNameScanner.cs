using ExcelColumnExtractor.Containers;
using ExcelColumnExtractor.NameObjects;
using ExcelDataReader;
using Microsoft.Extensions.Logging;

namespace ExcelColumnExtractor.Scanners;

public static class SheetNameScanner
{
    public static ExcelSheetNameContainer Scan(string excelPath, ILogger logger)
    {
        var sheetNames = new Dictionary<string, ExcelSheetName>();

        if (File.Exists(excelPath))
        {
            foreach (var sheetName in OnScan(excelPath, logger))
            {
                sheetNames.Add(sheetName.FullName, sheetName);
            }
        }
        else if (Directory.Exists(excelPath))
        {
            var files = Directory.GetFiles(excelPath, "*.xlsx")
                .Where(x => !Path.GetFileName(x).StartsWith("~$", StringComparison.Ordinal));

            foreach (var file in files)
            {
                foreach (var sheetName in OnScan(file, logger))
                {
                    sheetNames.Add(sheetName.FullName, sheetName);
                }
            }
        }
        else
        {
            throw new ArgumentException($"The file or directory does not exist. {nameof(excelPath)}");
        }

        return new(sheetNames);
    }

    private static List<ExcelSheetName> OnScan(string filePath, ILogger logger)
    {
        var sheetNames = new List<ExcelSheetName>();

        using var loader = new LockedFileStreamOpener(filePath);
        if (loader.IsTemp)
        {
            var lastWriteTime = File.GetLastWriteTime(filePath);
            LogInformation(logger, $"{nameof(SheetNameScanner)}: {Path.GetFileName(filePath)} 이미 열려있어 사본을 읽습니다. 마지막으로 저장된 시간: {lastWriteTime}", null);
        }

        using var reader = ExcelReaderFactory.CreateReader(loader.Stream);
        do
        {
            sheetNames.Add(new(filePath, reader.Name));
        }
        while (reader.NextResult());

        return sheetNames;
    }

    private static readonly Action<ILogger, string, Exception?> LogInformation =
        LoggerMessage.Define<string>(LogLevel.Information, new EventId(0, nameof(LogInformation)), "{Message}");
}
