using Microsoft.Extensions.Logging;
using SchemaInfoScanner;
using SchemaInfoScanner.Collectors;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.Schemata;
using Xunit.Abstractions;

namespace UnitTest;

public class RecordParameterSchemaFinderTest
{
    private readonly ITestOutputHelper testOutputHelper;

    public RecordParameterSchemaFinderTest(ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void RecordInTheRecordMultiWithListTest()
    {
        var code = @"
            public sealed record Subject(
                string Name,
                List<int> QuarterScore
            );

            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyClass(
                string Name,
                List<Subject> SubjectA,
                List<Subject> SubjectB,
            );";

        OnTest(code);
    }

    private void OnTest(string code)
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        Checker.Check(recordSchemaContainer, logger);

        var recordName = new RecordName("MyClass");
        RecordSchema recordSchema = recordSchemaContainer.RecordSchemaDictionary[recordName];

        var results = recordSchema.Find("Subject.QuarterScore", recordSchemaContainer);
        Assert.Equal(2, results.Count);
        Assert.Equal("MyClass.SubjectA", results[0].ParentParameterSchema!.ParameterName.FullName);
        Assert.Equal("MyClass.SubjectB", results[1].ParentParameterSchema!.ParameterName.FullName);
    }
}
