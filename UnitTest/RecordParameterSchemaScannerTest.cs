using Microsoft.Extensions.Logging;
using SchemaInfoScanner;
using SchemaInfoScanner.Collectors;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.Schemata;
using Xunit.Abstractions;

namespace UnitTest;

public class RecordParameterSchemaScannerTest
{
    private readonly ITestOutputHelper testOutputHelper;

    public RecordParameterSchemaScannerTest(ITestOutputHelper testOutputHelper)
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
                int Age,
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
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary[recordName];

        var results = recordSchema.Scan(recordSchemaContainer);
        Assert.Equal(6, results.Count);
        Assert.Equal("MyClass.Name", results[0].RecordParameterSchema.ParameterName.FullName);
        Assert.Null(results[0].ParentParameterSchema);
        Assert.Equal("Subject.Name", results[1].RecordParameterSchema.ParameterName.FullName);
        Assert.Equal("MyClass.SubjectA", results[1].ParentParameterSchema!.ParameterName.FullName);
        Assert.Equal("Subject.QuarterScore", results[2].RecordParameterSchema.ParameterName.FullName);
        Assert.Equal("MyClass.SubjectA", results[2].ParentParameterSchema!.ParameterName.FullName);
        Assert.Equal("MyClass.Age", results[3].RecordParameterSchema.ParameterName.FullName);
        Assert.Null(results[3].ParentParameterSchema);
        Assert.Equal("Subject.Name", results[4].RecordParameterSchema.ParameterName.FullName);
        Assert.Equal("MyClass.SubjectB", results[4].ParentParameterSchema!.ParameterName.FullName);
        Assert.Equal("Subject.QuarterScore", results[5].RecordParameterSchema.ParameterName.FullName);
        Assert.Equal("MyClass.SubjectB", results[5].ParentParameterSchema!.ParameterName.FullName);
    }
}
