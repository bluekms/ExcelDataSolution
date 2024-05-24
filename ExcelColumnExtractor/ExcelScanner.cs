using System.Collections.Frozen;
using ExcelColumnExtractor.NameObject;
using ExcelDataReader;
using Microsoft.Extensions.Logging;

namespace ExcelColumnExtractor;

public static class ExcelScanner
{
    private static readonly Action<ILogger, string, Exception?> LogInformation =
        LoggerMessage.Define<string>(LogLevel.Information, new EventId(0, nameof(LogInformation)), "{Message}");

    public static FrozenDictionary<string, SheetName> LoadExcelFiles(string excelPath, ILogger logger)
    {
        var sheetNames = new List<SheetName>();

        if (File.Exists(excelPath))
        {
            sheetNames.AddRange(OnLoadExcelFiles(excelPath, logger));
        }
        else if (Directory.Exists(excelPath))
        {
            var files = Directory.GetFiles(excelPath, "*.xlsx")
                .Where(x => !Path.GetFileName(x).StartsWith("~$", StringComparison.InvariantCulture));

            foreach (var file in files)
            {
                sheetNames.AddRange(OnLoadExcelFiles(file, logger));
            }
        }
        else
        {
            throw new ArgumentException($"The file or directory does not exist. {nameof(excelPath)}");
        }

        return UpdateDuplicateSheetNames(sheetNames).ToFrozenDictionary(x => x.Name);
    }

    private static IReadOnlyList<SheetName> UpdateDuplicateSheetNames(List<SheetName> sheetNames)
    {
        var encounteredNames = new HashSet<string>();
        var duplicates = sheetNames
            .Where(sheetName => !encounteredNames.Add(sheetName.Name))
            .ToList();

        var duplicateNames = duplicates.Select(x => x.Name);
        var updated = sheetNames
            .Where(x => duplicateNames.Contains(x.Name))
            .Select(x => new SheetName($"{Path.GetFileNameWithoutExtension(x.ExcelPath)}.{x.Name}", x.ExcelPath));

        var newList = new List<SheetName>();
        newList.AddRange(sheetNames.Where(x => !duplicateNames.Contains(x.Name)));
        newList.AddRange(updated);

        return newList;
    }

    private static IReadOnlyList<SheetName> OnLoadExcelFiles(string filePath, ILogger logger)
    {
        var sheetNames = new List<SheetName>();

        using var loader = new LockedFileStreamLoader(filePath);
        if (loader.IsTemp)
        {
            var lastWriteTime = File.GetLastWriteTime(filePath);
            LogInformation(logger, $"{Path.GetFileName(filePath)} 이미 열려있어 사본을 읽습니다. 마지막으로 저장된 시간: {lastWriteTime}", null);
        }

        using var reader = ExcelReaderFactory.CreateReader(loader.Stream);
        do
        {
            sheetNames.Add(new(reader.Name, filePath));
        }
        while (reader.NextResult());

        return sheetNames;
    }
}
