using System.Collections.Immutable;
using ExcelColumnExtractor.Checkers;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner;
using SchemaInfoScanner.Collectors;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.Exceptions;
using SchemaInfoScanner.NameObjects;
using Xunit.Abstractions;

namespace UnitTest;

public class ColumnNameAttributeTest
{
    private readonly ITestOutputHelper testOutputHelper;

    public ColumnNameAttributeTest(ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void NullablePrimitiveHashSetNotSupportedTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyRecord(
                string Name,
                [ColumnName(""Point"")] HashSet<int?> Score
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        Assert.Throws<InvalidUsageException>(() => Checker.Check(recordSchemaContainer, logger));
    }

    [Fact]
    public void NullablePrimitiveListNotSupportedTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyRecord(
                string Name,
                [ColumnName(""Point"")] List<int?> Score
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        Assert.Throws<InvalidUsageException>(() => Checker.Check(recordSchemaContainer, logger));
    }

    [Fact]
    public void NullablePrimitiveTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyRecord(
                string Name,
                [ColumnName(""Point"")] int? Score
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        Checker.Check(recordSchemaContainer, logger);

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
    public void NullableRecordHashSetNotSupportedTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            public sealed record Student(string Name, int Score);

            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyClass(
                string Name,
                [ColumnName(""Student"")] HashSet<Student?> Students
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        Assert.Throws<InvalidUsageException>(() => Checker.Check(recordSchemaContainer, logger));
    }

    [Fact]
    public void NullableRecordListNotSupportedTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            public sealed record Student(string Name, int Score);

            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyClass(
                string Name,
                [ColumnName(""Student"")] List<Student?> Students
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        Assert.Throws<InvalidUsageException>(() => Checker.Check(recordSchemaContainer, logger));
    }

    [Fact]
    public void NullableRecordTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            public sealed record Student(string Name, int Score);

            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyClass(
                string Name,
                [ColumnName(""Child"")] Student? Student
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        Checker.Check(recordSchemaContainer, logger);

        var recordName = new RecordName(".MyClass");
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary[recordName];

        var sheetHeaders = new List<string>
        {
            "Name", "Child.Name", "Child.Score"
        };

        HeaderChecker.Check(
            recordSchema.RecordParameterSchemaList,
            recordSchemaContainer,
            sheetHeaders.ToImmutableList(),
            new("./Test.xlsx", "TestSheet"));
    }

    [Fact]
    public void PrimitiveHashSetNotSupportedTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyRecord(
                string Name,
                [ColumnName(""Point"")] HashSet<int> Score
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        Assert.Throws<InvalidUsageException>(() => Checker.Check(recordSchemaContainer, logger));
    }

    [Fact]
    public void PrimitiveKeyRecordValueDictionaryNotSupportedTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            public sealed record Student([Key] string Name, int Score);

            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyRecord(
                [ColumnName(""Point"")] Dictionary<string, Student> Score
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        Assert.Throws<InvalidUsageException>(() => Checker.Check(recordSchemaContainer, logger));
    }

    [Fact]
    public void PrimitiveListNotSupportedTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyRecord(
                string Name,
                [ColumnName(""Point"")] List<int?> Score
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        Assert.Throws<InvalidUsageException>(() => Checker.Check(recordSchemaContainer, logger));
    }

    [Fact]
    public void PrimitiveTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyRecord(
                string Name,
                [ColumnName(""Point"")] int Score
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        Checker.Check(recordSchemaContainer, logger);

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
    public void RecordHashSetNotSupportedTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            public sealed record Student(string Name, int Score);

            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyClass(
                [ColumnName(""Student"")] HashSet<Student> Students
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        Assert.Throws<InvalidUsageException>(() => Checker.Check(recordSchemaContainer, logger));
    }

    [Fact]
    public void RecordKeyRecordValueDictionaryNotSupportedTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            public sealed record Student(int Id, string Name);

            public sealed record StudentScore([Key] Student Student, int Score);

            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyRecord(
                [ColumnName(""Point"")] Dictionary<Student, StudentScore> Score
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        Assert.Throws<InvalidUsageException>(() => Checker.Check(recordSchemaContainer, logger));
    }

    [Fact]
    public void RecordListNotSupportedTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            public sealed record Student(string Name, int Score);

            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyClass(
                [ColumnName(""Student"")] List<Student> Students
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        Assert.Throws<InvalidUsageException>(() => Checker.Check(recordSchemaContainer, logger));
    }

    [Fact]
    public void RecordTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            public sealed record Student(string Name, int Score);

            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyClass(
                string Name,
                [ColumnName(""Child"")] Student Student
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        Checker.Check(recordSchemaContainer, logger);

        var recordName = new RecordName(".MyClass");
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary[recordName];

        var sheetHeaders = new List<string>
        {
            "Name", "Child.Name", "Child.Score"
        };

        HeaderChecker.Check(
            recordSchema.RecordParameterSchemaList,
            recordSchemaContainer,
            sheetHeaders.ToImmutableList(),
            new("./Test.xlsx", "TestSheet"));
    }
}
