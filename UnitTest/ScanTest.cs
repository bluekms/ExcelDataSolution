using System.Reflection;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner;
using SchemaInfoScanner.Collectors;
using SchemaInfoScanner.Containers;
using Xunit.Abstractions;

namespace UnitTest;

public class ScanTest
{
    private readonly ITestOutputHelper testOutputHelper;

    public ScanTest(ITestOutputHelper testOutputHelper)
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

        var loadResultList = Loader.Load(csPath);

        var recordSchemaCollector = new RecordSchemaCollector();
        var enumMemberCollector = new EnumMemberCollector();

        foreach (var loadResult in loadResultList)
        {
            recordSchemaCollector.Collect(loadResult);
            enumMemberCollector.Collect(loadResult);
        }

        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);
        var enumMemberContainer = new EnumSchemaContainer(enumMemberCollector);

        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<ScanTest>();
        Checker.Check(recordSchemaContainer, logger);
    }
}
