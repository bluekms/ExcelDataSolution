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
    public void DictionaryTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            public sealed record Student(
                [Key] string Name,
                int Age
            );

            public sealed record MyRecord(
                Dictionary<string, Student> Students,
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        var recordName = new RecordName(".MyRecord");
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary[recordName];

        foreach (var parameterSchema in recordSchema.RecordParameterSchemaList)
        {
            DictionaryTypeChecker.Check(parameterSchema, recordSchemaContainer, new(), logger);
        }
    }

    [Fact]
    public void RecordKeyDictionaryTest()
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

            public sealed record MyRecord(
                Dictionary<Human, Student> Students,
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        var recordName = new RecordName(".MyRecord");
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary[recordName];

        foreach (var parameterSchema in recordSchema.RecordParameterSchemaList)
        {
            DictionaryTypeChecker.Check(parameterSchema, recordSchemaContainer, new(), logger);
        }
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

            public sealed record MyRecord(
                Dictionary<string, Student> Students,
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        var recordName = new RecordName(".MyRecord");
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary[recordName];

        foreach (var parameterSchema in recordSchema.RecordParameterSchemaList)
        {
            Assert.Throws<InvalidOperationException>(() =>
                DictionaryTypeChecker.Check(parameterSchema, recordSchemaContainer, new(), logger));
        }
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

            public sealed record MyRecord(
                Dictionary<int, Student> Students,
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        var recordName = new RecordName(".MyRecord");
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary[recordName];

        foreach (var parameterSchema in recordSchema.RecordParameterSchemaList)
        {
            Assert.Throws<TypeNotSupportedException>(() =>
                DictionaryTypeChecker.Check(parameterSchema, recordSchemaContainer, new(), logger));
        }
    }

    [Fact]
    public void PrimitiveValueDictionaryNotSupportedTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            public sealed record MyRecord(
                Dictionary<int, int> Values,
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary.Values.First();

        foreach (var parameterSchema in recordSchema.RecordParameterSchemaList)
        {
            Assert.Throws<TypeNotSupportedException>(() =>
                DictionaryTypeChecker.Check(parameterSchema, recordSchemaContainer, new(), logger));
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

            public sealed record MyRecord(
                Dictionary<string, Student>? Students,
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        var recordName = new RecordName(".MyRecord");
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary[recordName];

        foreach (var parameterSchema in recordSchema.RecordParameterSchemaList)
        {
            Assert.Throws<TypeNotSupportedException>(() =>
                DictionaryTypeChecker.Check(parameterSchema, recordSchemaContainer, new(), logger));
        }
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

            public sealed record MyRecord(
                Dictionary<string?, Student> Students,
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        var recordName = new RecordName(".MyRecord");
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary[recordName];

        foreach (var parameterSchema in recordSchema.RecordParameterSchemaList)
        {
            Assert.Throws<TypeNotSupportedException>(() =>
                DictionaryTypeChecker.Check(parameterSchema, recordSchemaContainer, new(), logger));
        }
    }

    [Fact]
    public void NullableRecordDictionaryNotSupportedTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            public sealed record Student(
                [Key] string Name,
                int Age
            );

            public sealed record MyRecord(
                Dictionary<string, Student?> Students,
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        var recordName = new RecordName(".MyRecord");
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary[recordName];

        foreach (var parameterSchema in recordSchema.RecordParameterSchemaList)
        {
            Assert.Throws<TypeNotSupportedException>(() =>
                DictionaryTypeChecker.Check(parameterSchema, recordSchemaContainer, new(), logger));
        }
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

            public sealed record MyRecord(
                Dictionary<List<int>, NestedRecord> Records,
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        var recordName = new RecordName(".MyRecord");
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary[recordName];

        foreach (var parameterSchema in recordSchema.RecordParameterSchemaList)
        {
            Assert.Throws<TypeNotSupportedException>(() =>
                DictionaryTypeChecker.Check(parameterSchema, recordSchemaContainer, new(), logger));
        }
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

            public sealed record MyRecord(
                Dictionary<int, List<DataRecord>> Records,
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        var recordName = new RecordName(".MyRecord");
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary[recordName];

        foreach (var parameterSchema in recordSchema.RecordParameterSchemaList)
        {
            Assert.Throws<TypeNotSupportedException>(() =>
                DictionaryTypeChecker.Check(parameterSchema, recordSchemaContainer, new(), logger));
        }
    }

    [Fact]
    public void SingleColumnContainerDictionaryNotSupportedTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            public sealed record Student(
                [Key] string Name,
                int Age
            );

            public sealed record MyRecord(
                [SingleColumnContainer("", "")] Dictionary<string, Student> Students,
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        var recordName = new RecordName(".MyRecord");
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary[recordName];

        foreach (var parameterSchema in recordSchema.RecordParameterSchemaList)
        {
            Assert.Throws<InvalidUsageException>(() =>
                DictionaryTypeChecker.Check(parameterSchema, recordSchemaContainer, new(), logger));
        }
    }
}
