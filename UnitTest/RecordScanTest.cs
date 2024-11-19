using System.Reflection;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner;
using SchemaInfoScanner.Collectors;
using SchemaInfoScanner.Containers;
using UnitTest.Utility;
using Xunit.Abstractions;

namespace UnitTest;

public class RecordScanTest
{
    private readonly ITestOutputHelper testOutputHelper;

    public RecordScanTest(ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void LoadAndCheckTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var csPath = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "..",
            "..",
            "..",
            "..",
            "_TestRecord");

        var loadResults = RecordSchemaLoader.Load(csPath, logger);

        var recordSchemaCollector = new RecordSchemaCollector();
        var enumMemberCollector = new EnumMemberCollector();
        var semanticModelCollector = new SemanticModelCollector();
        foreach (var loadResult in loadResults)
        {
            recordSchemaCollector.Collect(loadResult);
            enumMemberCollector.Collect(loadResult);
            semanticModelCollector.Collect(loadResult);
        }

        var enumMemberContainer = new EnumMemberContainer(enumMemberCollector);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector, enumMemberContainer);
        RecordComplianceChecker.Check(recordSchemaContainer, logger);
    }
}
