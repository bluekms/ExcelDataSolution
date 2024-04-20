using System.Globalization;
using System.Reflection;
using System.Text;
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

        var loadResultList = Loader.Load(csPath);
        var sb = new StringBuilder();
        foreach (var loadResult in loadResultList)
        {
            try
            {
                Checker.Check(loadResult.SemanticModel, loadResult.RecordDeclarations);
            }
            catch (Exception e)
            {
                sb.AppendLine(e.Message);
            }
        }

        // Scanner.Scan(csPath);

        if (sb.Length is not 0)
        {
            this.testOutputHelper.WriteLine(sb.ToString());
        }

        Assert.True(sb.Length is 0);
    }
}
