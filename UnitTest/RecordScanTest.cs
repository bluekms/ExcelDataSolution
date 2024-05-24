using System.Reflection;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner;
using SchemaInfoScanner.Collectors;
using SchemaInfoScanner.Containers;
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
        var csPath = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "..",
            "..",
            "..",
            "..",
            "_TestRecord");

        var loadResults = Loader.Load(csPath);

        var recordSchemaCollector = new RecordSchemaCollector();
        var enumMemberCollector = new EnumMemberCollector();
        var semanticModelCollector = new SemanticModelCollector();

        foreach (var loadResult in loadResults)
        {
            recordSchemaCollector.Collect(loadResult);
            enumMemberCollector.Collect(loadResult);
            semanticModelCollector.Collect(loadResult);
        }

        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);
        var enumMemberContainer = new EnumSchemaContainer(enumMemberCollector);

        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();
        Checker.Check(recordSchemaContainer, logger);
    }
}
