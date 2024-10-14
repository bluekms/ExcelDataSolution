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
using Xunit.Abstractions;

namespace UnitTest;

public class ExcelScanTest
{
    private static readonly Action<ILogger, string, Exception?> LogTrace =
        LoggerMessage.Define<string>(LogLevel.Trace, new EventId(0, nameof(LogTrace)), "{Message}");

    private static readonly Action<ILogger, string, Exception?> LogWarning =
        LoggerMessage.Define<string>(LogLevel.Warning, new EventId(0, nameof(LogWarning)), "{Message}");

    private readonly ITestOutputHelper testOutputHelper;

    public ExcelScanTest(ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void LoadTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var sheetNames = ScanExcelFiles(logger);

        testOutputHelper.WriteLine(sheetNames.Count.ToString(CultureInfo.InvariantCulture));
    }

    [Fact]
    public void LoadAndCompareRecordTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var sheetNameContainer = ScanExcelFiles(logger);
        var recordSchemaContainer = ScanRecordFiles(logger);

        foreach (var recordSchema in recordSchemaContainer.RecordSchemaDictionary.Values.OrderBy(x => x.RecordName.FullName))
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

    private static RecordSchemaContainer ScanRecordFiles(ILogger logger)
    {
        var csPath = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "..",
            "..",
            "..",
            "..",
            "TestRecord");

        var loadResults = RecordSchemaLoader.Load(csPath, logger);

        var recordSchemaCollector = new RecordSchemaCollector();
        foreach (var loadResult in loadResults)
        {
            recordSchemaCollector.Collect(loadResult);
        }

        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);
        RecordComplianceChecker.Check(recordSchemaContainer, logger);

        return recordSchemaContainer;
    }
}
