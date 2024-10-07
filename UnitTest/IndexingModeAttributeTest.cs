using System.Collections.Frozen;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner.Schemata.RecordSchemaExtensions;
using Xunit.Abstractions;

namespace UnitTest;

public class IndexingModeAttributeTest
{
    private readonly ITestOutputHelper testOutputHelper;

    public IndexingModeAttributeTest(ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void OneBasedTest()
    {
        var code = @"
            public sealed record Subject(
                string Name,
                List<int> QuarterScore
            );

            [StaticDataRecord(""Test"", ""TestSheet"")]
            [IndexingMode(IndexingMode.OneBased)]
            public sealed record MyClass(
                string Name,
                List<Subject> SubjectA,
                int Age,
                List<Subject> SubjectB,
            );";

        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var parseResult = SimpleCordParser.Parse(code, logger);

        var collectionLengths = new Dictionary<string, int>
        {
            { "SubjectA", 3 },
            { "SubjectA.QuarterScore", 4 },
            { "SubjectB", 4 },
            { "SubjectB.QuarterScore", 2 },
        };

        var results = parseResult.RecordSchema.Flatten(parseResult.RecordSchemaContainer, collectionLengths.ToFrozenDictionary(), logger);
        Assert.Equal("SubjectA[1].Name", results[1]);
        Assert.Equal("SubjectA[1].QuarterScore[0]", results[2]);
        Assert.Equal("SubjectA[1].QuarterScore[3]", results[5]);
        Assert.Equal("SubjectA[3].QuarterScore[3]", results[15]);
        Assert.Equal("SubjectB[1].Name", results[17]);
        Assert.Equal("SubjectB[1].QuarterScore[0]", results[18]);
        Assert.Equal("SubjectB[1].QuarterScore[1]", results[19]);
        Assert.Equal("SubjectB[4].QuarterScore[1]", results[28]);
    }

    [Fact]
    public void InnerOneBaseTest()
    {
        var code = @"
            [IndexingMode(IndexingMode.OneBased)]
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

        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var parseResult = SimpleCordParser.Parse(code, logger);

        var collectionLengths = new Dictionary<string, int>
        {
            { "SubjectA", 3 },
            { "SubjectA.QuarterScore", 4 },
            { "SubjectB", 4 },
            { "SubjectB.QuarterScore", 2 },
        };

        var results = parseResult.RecordSchema.Flatten(parseResult.RecordSchemaContainer, collectionLengths.ToFrozenDictionary(), logger);
        Assert.Equal("SubjectA[0].Name", results[1]);
        Assert.Equal("SubjectA[0].QuarterScore[1]", results[2]);
        Assert.Equal("SubjectA[0].QuarterScore[4]", results[5]);
        Assert.Equal("SubjectA[2].QuarterScore[4]", results[15]);
        Assert.Equal("SubjectB[0].Name", results[17]);
        Assert.Equal("SubjectB[0].QuarterScore[1]", results[18]);
        Assert.Equal("SubjectB[0].QuarterScore[2]", results[19]);
        Assert.Equal("SubjectB[3].QuarterScore[2]", results[28]);
    }

    [Fact]
    public void GlobalOneBaseTest()
    {
        var code = @"
            public sealed record Subject(
                string Name,
                List<int> QuarterScore
            );

            [StaticDataRecord(""Test"", ""TestSheet"")]
            [RecordGlobalIndexingMode(IndexingMode.OneBased)]
            public sealed record MyClass(
                string Name,
                List<Subject> SubjectA,
                int Age,
                List<Subject> SubjectB,
            );";

        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var parseResult = SimpleCordParser.Parse(code, logger);

        var collectionLengths = new Dictionary<string, int>
        {
            { "SubjectA", 3 },
            { "SubjectA.QuarterScore", 4 },
            { "SubjectB", 4 },
            { "SubjectB.QuarterScore", 2 },
        };

        var results = parseResult.RecordSchema.Flatten(parseResult.RecordSchemaContainer, collectionLengths.ToFrozenDictionary(), logger);
        Assert.Equal("SubjectA[1].Name", results[1]);
        Assert.Equal("SubjectA[1].QuarterScore[1]", results[2]);
        Assert.Equal("SubjectA[1].QuarterScore[4]", results[5]);
        Assert.Equal("SubjectA[3].QuarterScore[4]", results[15]);
        Assert.Equal("SubjectB[1].Name", results[17]);
        Assert.Equal("SubjectB[1].QuarterScore[1]", results[18]);
        Assert.Equal("SubjectB[1].QuarterScore[2]", results[19]);
        Assert.Equal("SubjectB[4].QuarterScore[2]", results[28]);
    }

    [Fact]
    public void GlobalOneBase2Test()
    {
        var codeOneBased = @"
            [IndexingMode(IndexingMode.OneBased)]
            public sealed record Subject(
                string Name,
                List<int> QuarterScore
            );

            [StaticDataRecord(""Test"", ""TestSheet"")]
            [IndexingMode(IndexingMode.OneBased)]
            public sealed record MyClass(
                string Name,
                List<Subject> SubjectA,
                int Age,
                List<Subject> SubjectB,
            );";

        var codeGlobal = @"
            public sealed record Subject(
                string Name,
                List<int> QuarterScore
            );

            [StaticDataRecord(""Test"", ""TestSheet"")]
            [RecordGlobalIndexingMode(IndexingMode.OneBased)]
            public sealed record MyClass(
                string Name,
                List<Subject> SubjectA,
                int Age,
                List<Subject> SubjectB,
            );";

        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var parseOneBased = SimpleCordParser.Parse(codeOneBased, logger);
        var parseGlobal = SimpleCordParser.Parse(codeGlobal, logger);

        var collectionLengths = new Dictionary<string, int>
        {
            { "SubjectA", 3 },
            { "SubjectA.QuarterScore", 4 },
            { "SubjectB", 4 },
            { "SubjectB.QuarterScore", 2 },
        };

        var resultsOneBased = parseGlobal.RecordSchema.Flatten(parseOneBased.RecordSchemaContainer, collectionLengths.ToFrozenDictionary(), logger);
        var resultsGlobal = parseGlobal.RecordSchema.Flatten(parseGlobal.RecordSchemaContainer, collectionLengths.ToFrozenDictionary(), logger);
        Assert.Equal(resultsOneBased, resultsGlobal);
    }
}
