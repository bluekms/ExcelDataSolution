using System.Collections.Immutable;
using ExcelColumnExtractor.Checkers;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner;
using SchemaInfoScanner.Collectors;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.NameObjects;
using Xunit.Abstractions;

namespace UnitTest;

public class HeaderCheckerTest
{
    private readonly ITestOutputHelper testOutputHelper;

    public HeaderCheckerTest(ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void Test()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            public sealed record MyRecord(
                string Name,
                int Score
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);
        var recordName = new RecordName(".MyRecord");
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary[recordName];

        var sheetHeaders = new List<string>
        {
            "Name", "Score"
        };

        HeaderChecker.Check(
            recordSchema.RecordParameterSchemaList,
            recordSchemaContainer,
            sheetHeaders.ToImmutableList(),
            new("./Test.xlsx", "TestSheet"));
    }

    [Fact]
    public void ColumnNameAttributeTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            public sealed record MyRecord(
                string Name,
                [ColumnName(""Point"")] int Score
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);
        var recordName = new RecordName(".MyRecord");
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary[recordName];

        var sheetHeaders = new List<string>
        {
            "Name", "Point"
        };

        HeaderChecker.Check(
            recordSchema.RecordParameterSchemaList,
            recordSchemaContainer,
            sheetHeaders.ToImmutableList(),
            new("./Test.xlsx", "TestSheet"));
    }

    [Fact]
    public void ListTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            public sealed record MyRecord(
                string Name,
                List<int> Score
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);
        var recordName = new RecordName(".MyRecord");
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary[recordName];

        var sheetHeaders = new List<string>
        {
            "Name", "Score1", "Score2", "Score3"
        };

        HeaderChecker.Check(
            recordSchema.RecordParameterSchemaList,
            recordSchemaContainer,
            sheetHeaders.ToImmutableList(),
            new("./Test.xlsx", "TestSheet"));
    }

    [Fact]
    public void MaxCountTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            public sealed record MyRecord(
                string Name,
                [MaxCount(3)] List<int> Score
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);
        var recordName = new RecordName(".MyRecord");
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary[recordName];

        var sheetHeaders = new List<string>
        {
            "Name", "Score1", "Score2", "Score3"
        };

        HeaderChecker.Check(
            recordSchema.RecordParameterSchemaList,
            recordSchemaContainer,
            sheetHeaders.ToImmutableList(),
            new("./Test.xlsx", "TestSheet"));
    }

    /*
    [Fact]
    public void MaxCountRejectsTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            public sealed record MyRecord(
                string Name,
                [MaxCount(2)] List<int> Score
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);
        var recordName = new RecordName(".MyRecord");
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary[recordName];

        var sheetHeaders = new List<string>
        {
            "Name", "Score1", "Score2", "Score3"
        };

        Assert.Throws<InvalidDataException>(() =>
        {
            HeaderChecker.Check(
                recordSchema.RecordParameterSchemaList,
                recordSchemaContainer,
                sheetHeaders.ToImmutableList(),
                new("./Test.xlsx", "TestSheet"));
        });
    }
    */

    [Fact]
    public void ColumnPrefixAttributeTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            public sealed record MyRecord(
                [ColumnPrefix(""Point"")] List<int> Scores
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);
        var recordName = new RecordName(".MyRecord");
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary[recordName];

        var sheetHeaders = new List<string>
        {
            "Point1", "Point2", "Point3"
        };

        HeaderChecker.Check(
            recordSchema.RecordParameterSchemaList,
            recordSchemaContainer,
            sheetHeaders.ToImmutableList(),
            new("./Test.xlsx", "TestSheet"));
    }

    [Fact]
    public void SingleColumnContainerAttributeTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            public sealed record MyRecord(
                string Name,
                [SingleColumnContainer("", "")] List<int> Scores
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);
        var recordName = new RecordName(".MyRecord");
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary[recordName];

        var sheetHeaders = new List<string>
        {
            "Name", "Scores"
        };

        HeaderChecker.Check(
            recordSchema.RecordParameterSchemaList,
            recordSchemaContainer,
            sheetHeaders.ToImmutableList(),
            new("./Test.xlsx", "TestSheet"));
    }

    [Fact]
    public void SingleColumnContainerWithColumnNameTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            public sealed record MyRecord(
                string Name,
                [ColumnName(""Points"")][SingleColumnContainer("", "")] List<int> Scores
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);
        var recordName = new RecordName(".MyRecord");
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary[recordName];

        var sheetHeaders = new List<string>
        {
            "Name", "Points"
        };

        HeaderChecker.Check(
            recordSchema.RecordParameterSchemaList,
            recordSchemaContainer,
            sheetHeaders.ToImmutableList(),
            new("./Test.xlsx", "TestSheet"));
    }

    [Fact]
    public void RecordContainerTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            public sealed record Student(
                string Name,
                int Score
            );

            public sealed record MyRecord(
                int Id,
                HashSet<Student> Students
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);
        var recordName = new RecordName(".MyRecord");
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary[recordName];

        var sheetHeaders = new List<string>
        {
            "Id", "Students1.Name", "Students1.Score", "Students2.Name", "Students2.Score", "Students3.Name", "Students3.Score"
        };

        HeaderChecker.Check(
            recordSchema.RecordParameterSchemaList,
            recordSchemaContainer,
            sheetHeaders.ToImmutableList(),
            new("./Test.xlsx", "TestSheet"));
    }
}
