using System.Reflection;
using SchemaInfoScanner;
using SchemaInfoScanner.Collectors;
using SchemaInfoScanner.Containers;
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
        var enumMemberCollector = new EnumMemberCollector();
        var semanticModelCollector = new SemanticModelCollector();

        foreach (var loadResult in loadResultList)
        {
            recordSchemaCollector.Collect(loadResult);
            enumMemberCollector.Collect(loadResult);
            semanticModelCollector.Collect(loadResult);
        }

        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);
        var enumMemberContainer = new EnumSchemaContainer(enumMemberCollector);
        var semanticModelContainer = new SemanticModelContainer(semanticModelCollector);

        var log = new List<string>();

        try
        {
            Checker.Check(recordSchemaContainer, semanticModelContainer, log);
        }
        catch (Exception e)
        {
            foreach (var s in log)
            {
                testOutputHelper.WriteLine(s);
            }

            testOutputHelper.WriteLine(e.Message);
            throw;
        }
    }
}
