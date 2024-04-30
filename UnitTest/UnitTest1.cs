using System.Reflection;
using System.Text;
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

        var sb = new StringBuilder();

        var loadResultList = Loader.Load(csPath);
        var recordSchemaCollector = new RecordSchemaCollector();
        foreach (var loadResult in loadResultList)
        {
            recordSchemaCollector.Collect(loadResult);
        }

        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        if (sb.Length is not 0)
        {
            this.testOutputHelper.WriteLine(sb.ToString());
        }

        Assert.True(sb.Length is 0);
    }
}
