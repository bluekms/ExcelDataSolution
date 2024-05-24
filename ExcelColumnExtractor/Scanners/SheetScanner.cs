using System.Collections.Frozen;
using ExcelColumnExtractor.NameObjects;
using ExcelDataReader;
using Microsoft.Extensions.Logging;

namespace ExcelColumnExtractor.Scanners;

public static class SheetScanner
{
    private static readonly Action<ILogger, string, Exception?> LogInformation =
        LoggerMessage.Define<string>(LogLevel.Information, new EventId(0, nameof(LogInformation)), "{Message}");

    public static FrozenDictionary<string, SheetName> Scan(string excelPath, ILogger logger)
    {
        var sheetNames = new Dictionary<string, SheetName>();

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
                .Where(x => !Path.GetFileName(x).StartsWith("~$", StringComparison.InvariantCulture));

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

        return sheetNames.ToFrozenDictionary();
    }

    private static IReadOnlyList<SheetName> OnScan(string filePath, ILogger logger)
    {
        var sheetNames = new List<SheetName>();

        using var loader = new LockedFileStreamLoader(filePath);
        if (loader.IsTemp)
        {
            var lastWriteTime = File.GetLastWriteTime(filePath);
            LogInformation(logger, $"{nameof(SheetScanner)}: {Path.GetFileName(filePath)} 이미 열려있어 사본을 읽습니다. 마지막으로 저장된 시간: {lastWriteTime}", null);
        }

        using var reader = ExcelReaderFactory.CreateReader(loader.Stream);
        do
        {
            sheetNames.Add(new(filePath, reader.Name));
        }
        while (reader.NextResult());

        return sheetNames;
    }
}
