using System.Collections.Immutable;
using ExcelColumnExtractor.Checkers;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner;
using SchemaInfoScanner.Collectors;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.NameObjects;
using Xunit.Abstractions;

namespace UnitTest;

public class ColumnPrefixAttributeTest
{
    private readonly ITestOutputHelper testOutputHelper;

    public ColumnPrefixAttributeTest(ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void RecordKeyRecordValueInListDictionaryTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            public sealed record Student(int Id, string Name, int Age);

            public sealed record StudentScore([Key] Student Student, [ColumnPrefix(""Score"")] List<int> Subject);

            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyRecord(
                Dictionary<Student, StudentScore> Score
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        Checker.Check(recordSchemaContainer, logger);

        var recordName = new RecordName(".MyRecord");
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary[recordName];

        var sheetHeaders = new List<string>
        {
            "Score1.Id", "Score1.Name", "Score1.Age", "Score1.Subject1", "Score1.Subject2", "Score1.Subject3",
            "Score2.Id", "Score2.Name", "Score2.Age", "Score2.Subject1", "Score2.Subject2", "Score2.Subject3",
        };

        HeaderChecker.Check(
            recordSchema.RecordParameterSchemaList,
            recordSchemaContainer,
            sheetHeaders.ToImmutableList(),
            new("./Test.xlsx", "TestSheet"));
    }
}
