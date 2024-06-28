using System.Collections.Immutable;
using ExcelColumnExtractor.Checkers;
using ExcelColumnExtractor.Scanners;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner;
using SchemaInfoScanner.Collectors;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.NameObjects;
using Xunit.Abstractions;

namespace UnitTest;

public class Playground
{
    private readonly ITestOutputHelper testOutputHelper;

    public Playground(ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void Test()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            public sealed record Subject(
                string Name,
                List<int> QuarterScore,
            );

            public sealed record Student(
                [Key] int Id,
                string Name,
                List<Subject> Subjects,
            );

            public enum ContactType
            {
                Phone,
                Email,
                Address,
            }

            public sealed record Contacts(ContactType Type, string Value);

            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyClass(
                string Name,
                Contacts Contact,
                Dictionary<int, Student> Students,
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        Checker.Check(recordSchemaContainer, logger);

        var recordName = new RecordName("MyClass");
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary[recordName];

        var sheetHeaders = new List<string>
        {
            "Name",
            "Contact.Type",
            "Contact.Value",
            "Students1.Id",
            "Students1.Name",
            "Students1.Subjects1.Name",
            "Students1.Subjects1.QuarterScore1",
            "Students1.Subjects1.QuarterScore2",
            "Students1.Subjects1.QuarterScore3",
            "Students1.Subjects1.QuarterScore4",
            "Students1.Subjects2.Name",
            "Students1.Subjects2.QuarterScore1",
            "Students1.Subjects2.QuarterScore2",
            "Students1.Subjects2.QuarterScore3",
            "Students1.Subjects2.QuarterScore4",
            "Students1.Subjects3.Name",
            "Students1.Subjects3.QuarterScore1",
            "Students1.Subjects3.QuarterScore2",
            "Students1.Subjects3.QuarterScore3",
            "Students1.Subjects3.QuarterScore4",
            "Students2.Id",
            "Students2.Name",
            "Students2.Subjects1.Name",
            "Students2.Subjects1.QuarterScore1",
            "Students2.Subjects1.QuarterScore2",
            "Students2.Subjects1.QuarterScore3",
            "Students2.Subjects1.QuarterScore4",
            "Students2.Subjects2.Name",
            "Students2.Subjects2.QuarterScore1",
            "Students2.Subjects2.QuarterScore2",
            "Students2.Subjects2.QuarterScore3",
            "Students2.Subjects2.QuarterScore4",
            "Students2.Subjects3.Name",
            "Students2.Subjects3.QuarterScore1",
            "Students2.Subjects3.QuarterScore2",
            "Students2.Subjects3.QuarterScore3",
            "Students2.Subjects3.QuarterScore4",
            "Students3.Id",
            "Students3.Name",
            "Students3.Subjects1.Name",
            "Students3.Subjects1.QuarterScore1",
            "Students3.Subjects1.QuarterScore2",
            "Students3.Subjects1.QuarterScore3",
            "Students3.Subjects1.QuarterScore4",
            "Students3.Subjects2.Name",
            "Students3.Subjects2.QuarterScore1",
            "Students3.Subjects2.QuarterScore2",
            "Students3.Subjects2.QuarterScore3",
            "Students3.Subjects2.QuarterScore4",
            "Students3.Subjects3.Name",
            "Students3.Subjects3.QuarterScore1",
            "Students3.Subjects3.QuarterScore2",
            "Students3.Subjects3.QuarterScore3",
            "Students3.Subjects3.QuarterScore4",
        };

        HeaderChecker.Check(sheetHeaders.ToImmutableList(), recordSchema, recordSchemaContainer, logger);
    }
}
