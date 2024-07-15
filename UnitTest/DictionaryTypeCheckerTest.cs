using Microsoft.Extensions.Logging;
using SchemaInfoScanner;
using SchemaInfoScanner.Collectors;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.Exceptions;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.TypeCheckers;
using Xunit.Abstractions;

namespace UnitTest;

public class DictionaryTypeCheckerTest
{
    private readonly ITestOutputHelper testOutputHelper;

    public DictionaryTypeCheckerTest(ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void PrimitiveKeyRecordValueDictionaryTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            public sealed record Student(
                [Key] string Name,
                int Age
            );

            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyRecord(
                Dictionary<string, Student> Students,
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        Checker.Check(recordSchemaContainer, logger);

        var recordName = new RecordName("MyRecord");
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary[recordName];

        foreach (var parameterSchema in recordSchema.RecordParameterSchemaList)
        {
            DictionaryTypeChecker.Check(parameterSchema, recordSchemaContainer, new(), logger);
        }
    }

    [Fact]
    public void RecordKeyRecordValueDictionaryTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            public sealed record Human(
                string Name,
                int Age
            );

            public sealed record Student(
                [Key] Human Human,
                int Grade
            );

            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyRecord(
                Dictionary<Human, Student> Students,
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        Checker.Check(recordSchemaContainer, logger);

        var recordName = new RecordName("MyRecord");
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary[recordName];

        foreach (var parameterSchema in recordSchema.RecordParameterSchemaList)
        {
            DictionaryTypeChecker.Check(parameterSchema, recordSchemaContainer, new(), logger);
        }
    }

    [Fact]
    public void NullableDictionaryNotSupportedTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            public sealed record Student(
                [Key] string Name,
                int Age
            );

            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyRecord(
                Dictionary<string, Student>? Students,
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        Assert.Throws<TypeNotSupportedException>(() => Checker.Check(recordSchemaContainer, logger));
    }

    [Fact]
    public void PrimitiveValueDictionaryNotSupportedTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyRecord(
                Dictionary<int, int> Values,
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        Assert.Throws<TypeNotSupportedException>(() => Checker.Check(recordSchemaContainer, logger));
    }

    [Fact]
    public void NullableKeyDictionaryNotSupportedTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            public sealed record Student(
                [Key] string Name,
                int Age
            );

            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyRecord(
                Dictionary<string?, Student> Students,
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        Assert.Throws<TypeNotSupportedException>(() => Checker.Check(recordSchemaContainer, logger));
    }

    [Fact]
    public void NullableRecordValueDictionaryNotSupportedTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            public sealed record Student(
                [Key] string Name,
                int Age
            );

            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyRecord(
                Dictionary<string, Student?> Students,
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        Assert.Throws<TypeNotSupportedException>(() => Checker.Check(recordSchemaContainer, logger));
    }

    [Fact]
    public void RecordWithoutKeyDictionaryNotSupportedTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            public sealed record Student(
                string Name,
                int Age
            );

            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyRecord(
                Dictionary<string, Student> Students,
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        Assert.Throws<InvalidUsageException>(() => Checker.Check(recordSchemaContainer, logger));
    }

    [Fact]
    public void RecordWithDifferentKeyTypesDictionaryNotSupportedTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            public sealed record Student(
                [Key] string Name,
                int Age
            );

            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyRecord(
                Dictionary<int, Student> Students,
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        Assert.Throws<TypeNotSupportedException>(() => Checker.Check(recordSchemaContainer, logger));
    }

    [Fact]
    public void NestedContainerKeyDictionaryNotSupportedTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            public sealed record NestedRecord(
                [Key] List<int> Values,
                string Data,
            );

            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyRecord(
                Dictionary<List<int>, NestedRecord> Records,
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        Assert.Throws<InvalidUsageException>(() => Checker.Check(recordSchemaContainer, logger));
    }

    [Fact]
    public void NestedContainerValueDictionaryNotSupportedTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            public sealed record DataRecord(
                [Key] int Id,
                string Data,
            );

            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyRecord(
                Dictionary<int, List<DataRecord>> Records,
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        Assert.Throws<TypeNotSupportedException>(() => Checker.Check(recordSchemaContainer, logger));
    }

    [Fact]
    public void ImmutableDictionaryTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            public sealed record Student(
                [Key] string Name,
                int Age
            );

            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyRecord(
                Dictionary<string, Student> DataA,
                ImmutableDictionary<string, Student> DataB,
                ImmutableSortedDictionary<string, Student> DataC,
                FrozenDictionary<string, Student> DataD,
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        Checker.Check(recordSchemaContainer, logger);

        var recordName = new RecordName("MyRecord");
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary[recordName];

        foreach (var parameterSchema in recordSchema.RecordParameterSchemaList)
        {
            DictionaryTypeChecker.Check(parameterSchema, recordSchemaContainer, new(), logger);
        }
    }
}
