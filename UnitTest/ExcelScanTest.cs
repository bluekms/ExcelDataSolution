using System.Globalization;
using ExcelColumnExtractor.Scanners;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner;
using SchemaInfoScanner.Catalogs;
using SchemaInfoScanner.Collectors;
using SchemaInfoScanner.Extensions;
using Sdp.Attributes;
using UnitTest.Utility;
using Xunit.Abstractions;

namespace UnitTest;

[Collection("ExcelFileTests")]
public class ExcelScanTest(ITestOutputHelper testOutputHelper)
{
    private const string Excel3RecordsResourceFileName = "Excel3Records.cs";

    private static readonly string[] ExcelResourceFileNames =
    [
        "Excel1.xlsx",
        "Excel2.xlsx",
        "Excel3.xlsx",
    ];

    private static readonly Action<ILogger, string, Exception?> LogTrace =
        LoggerMessage.Define<string>(LogLevel.Trace, new EventId(0, nameof(LogTrace)), "{Message}");

    private static readonly Action<ILogger, string, Exception?> LogWarning =
        LoggerMessage.Define<string>(LogLevel.Warning, new EventId(0, nameof(LogWarning)), "{Message}");

    [Fact]
    public void LoadTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<ExcelScanTest>() is not TestOutputLogger<ExcelScanTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        using var excelData = new TestDataDirectory(ExcelResourceFileNames);
        var sheetNames = SheetNameScanner.Scan(excelData.Path, logger);

        testOutputHelper.WriteLine(sheetNames.Count.ToString(CultureInfo.InvariantCulture));
        Assert.Empty(logger.Logs);
    }

    [Fact]
    public void LoadAndCompareRecordTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<ExcelScanTest>() is not TestOutputLogger<ExcelScanTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        using var excelData = new TestDataDirectory(ExcelResourceFileNames);
        using var recordData = new TestDataDirectory(Excel3RecordsResourceFileName);

        var sheetNameCollection = SheetNameScanner.Scan(excelData.Path, logger);
        var recordSchemaCatalog = ScanRecordFile(recordData.GetFilePath(Excel3RecordsResourceFileName), logger);

        foreach (var recordSchema in recordSchemaCatalog.StaticDataRecordSchemata)
        {
            if (!recordSchema.HasAttribute<StaticDataRecordAttribute>())
            {
                continue;
            }

            var values = recordSchema.GetAttributeValueList<StaticDataRecordAttribute>();
            var sheetNameString = $"{values[0]}.{values[1]}";

            if (sheetNameCollection.TryGet(values[0], values[1], out _))
            {
                LogTrace(logger, $"Match! {sheetNameString} : {recordSchema.RecordName.FullName}", null);
            }
            else
            {
                LogWarning(logger, $"Not found sheet {sheetNameString}.", null);
            }
        }

        Assert.Empty(logger.Logs);
    }

    private static RecordSchemaCatalog ScanRecordFile(string csPath, ILogger logger)
    {
        var loadResults = RecordSchemaLoader.Load(csPath, logger);
        var recordSchemaSet = new RecordSchemaSet(loadResults, logger);

        var recordSchemaCatalog = new RecordSchemaCatalog(recordSchemaSet);
        RecordComplianceChecker.Check(recordSchemaCatalog, logger);

        return recordSchemaCatalog;
    }
}
