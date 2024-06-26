using System.Collections.Frozen;
using System.Globalization;
using System.Reflection;
using ExcelColumnExtractor.NameObjects;
using ExcelColumnExtractor.Scanners;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner;
using SchemaInfoScanner.Collectors;
using SchemaInfoScanner.Containers;
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

        var sheetNameDictionary = ScanExcelFiles(logger);
        var recordSchemaContainer = ScanRecordFiles(logger);

        foreach (var recordSchema in recordSchemaContainer.RecordSchemaDictionary.Values.OrderBy(x => x.RecordName.FullName))
        {
            if (!recordSchema.HasAttribute<StaticDataRecordAttribute>())
            {
                continue;
            }

            var sheetName = new SheetName(
                recordSchema.GetAttributeValue<StaticDataRecordAttribute, string>(0),
                recordSchema.GetAttributeValue<StaticDataRecordAttribute, string>(1));
            if (sheetNameDictionary.ContainsKey(sheetName.FullName))
            {
                LogTrace(logger, $"Match! {sheetName.FullName} : {recordSchema.RecordName.FullName}", null);
            }
            else
            {
                LogWarning(logger, $"Not found sheet {sheetName.FullName}.", null);
            }
        }
    }

    private static FrozenDictionary<string, SheetName> ScanExcelFiles(ILogger logger)
    {
        var excelPath = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "..",
            "..",
            "..",
            "..",
            "TestExcel");

        return SheetScanner.Scan(excelPath, logger);
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

        var loadResults = Loader.Load(csPath, logger);

        var recordSchemaCollector = new RecordSchemaCollector();
        foreach (var loadResult in loadResults)
        {
            recordSchemaCollector.Collect(loadResult);
        }

        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);
        Checker.Check(recordSchemaContainer, logger);

        return recordSchemaContainer;
    }
}
