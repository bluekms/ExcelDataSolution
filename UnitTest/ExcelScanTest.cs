using System.Globalization;
using System.Reflection;
using ExcelColumnExtractor.Containers;
using ExcelColumnExtractor.Scanners;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner;
using SchemaInfoScanner.Collectors;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.Extensions;
using StaticDataAttribute;
using UnitTest.Utility;
using Xunit.Abstractions;

namespace UnitTest;

public class ExcelScanTest(ITestOutputHelper testOutputHelper)
{
    private static readonly Action<ILogger, string, Exception?> LogTrace =
        LoggerMessage.Define<string>(LogLevel.Trace, new EventId(0, nameof(LogTrace)), "{Message}");

    private static readonly Action<ILogger, string, Exception?> LogWarning =
        LoggerMessage.Define<string>(LogLevel.Warning, new EventId(0, nameof(LogWarning)), "{Message}");

    [Fact]
    public void LoadTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Trace);
        if (factory.CreateLogger<ExcelScanTest>() is not TestOutputLogger<ExcelScanTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var sheetNames = ScanExcelFiles(logger);

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

        var sheetNameContainer = ScanExcelFiles(logger);
        var recordSchemaCatalog = ScanRecordFiles(logger);

        foreach (var recordSchema in recordSchemaCatalog.StaticDataRecordSchemata)
        {
            if (!recordSchema.HasAttribute<StaticDataRecordAttribute>())
            {
                continue;
            }

            var values = recordSchema.GetAttributeValueList<StaticDataRecordAttribute>();
            var sheetNameString = $"{values[0]}.{values[1]}";

            if (sheetNameContainer.TryGet(values[0], values[1], out _))
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

    private static ExcelSheetNameContainer ScanExcelFiles(ILogger logger)
    {
        var excelPath = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "..",
            "..",
            "..",
            "..",
            "TestExcel");

        return SheetNameScanner.Scan(excelPath, logger);
    }

    private static RecordSchemaCatalog ScanRecordFiles(ILogger logger)
    {
        var csPath = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "..",
            "..",
            "..",
            "..",
            "TestRecord");

        var loadResults = RecordSchemaLoader.Load(csPath, logger);
        var recordSchemaSet = new RecordSchemaSet(loadResults, logger);

        var recordSchemaCatalog = new RecordSchemaCatalog(recordSchemaSet);
        RecordComplianceChecker.Check(recordSchemaCatalog, logger);

        return recordSchemaCatalog;
    }
}
