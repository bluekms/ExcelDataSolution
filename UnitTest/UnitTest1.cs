using System.Reflection;
using SchemaInfoScanner;
using SchemaInfoScanner.Collectors;
using Xunit.Abstractions;

namespace UnitTest;

public class UnitTest1
{
    private readonly ITestOutputHelper testOutputHelper;

    public UnitTest1(ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void ScanTest()
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
        foreach (var loadResult in loadResultList)
        {
            recordSchemaCollector.Collect(loadResult);
        }

        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        // Checker.Check(recordSchemaContainer);
    }
}
