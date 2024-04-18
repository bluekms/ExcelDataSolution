using System.Reflection;
using SchemaInfoScanner;
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
    public void Test1()
    {
        var csPath = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "..",
            "..",
            "..",
            "..",
            "_TestRecord");
        try
        {
            var loadResultList = Loader.Load(csPath);
            foreach (var loadResult in loadResultList)
            {
                Checker.Check(loadResult.SemanticModel, loadResult.RecordDeclarations);
            }

            // Scanner.Scan(csPath);
        }
        catch (Exception e)
        {
            testOutputHelper.WriteLine(e.ToString());
            throw;
        }

        Assert.True(true);
    }
}
