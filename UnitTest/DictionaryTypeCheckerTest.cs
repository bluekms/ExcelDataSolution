using Microsoft.Extensions.Logging;
using SchemaInfoScanner;
using SchemaInfoScanner.Collectors;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.Exceptions;
using SchemaInfoScanner.TypeCheckers;
using UnitTest.Utility;
using Xunit.Abstractions;

namespace UnitTest;

public class DictionaryTypeCheckerTest(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void PrimitiveKeyPrimitiveValueDictionaryTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<DictionaryTypeCheckerTest>() is not TestOutputLogger<DictionaryTypeCheckerTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = @"
            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyRecord(
                Dictionary<int, string> Students,
            );";

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaSet = new RecordSchemaSet(loadResult);
        var recordSchemaCatalog = new RecordSchemaCatalog(recordSchemaSet);
        RecordComplianceChecker.Check(recordSchemaCatalog, logger);

        Assert.Empty(logger.Logs);
    }

    [Fact]
    public void PrimitiveKeyRecordValueDictionaryTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<DictionaryTypeCheckerTest>() is not TestOutputLogger<DictionaryTypeCheckerTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = @"
            public sealed record Student(
                [Key] string Name,
                int Age
            );

            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyRecord(
                Dictionary<string, Student> Students,
            );";

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaSet = new RecordSchemaSet(loadResult);
        var recordSchemaCatalog = new RecordSchemaCatalog(recordSchemaSet);
        RecordComplianceChecker.Check(recordSchemaCatalog, logger);

        var recordSchema = recordSchemaCatalog.StaticDataRecordSchemata[0];
        foreach (var parameterSchema in recordSchema.RecordPropertySchemata)
        {
            DictionaryTypeChecker.Check(parameterSchema, recordSchemaCatalog, [], logger);
        }

        Assert.Empty(logger.Logs);
    }

    [Fact]
    public void RecordKeyRecordValueDictionaryTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<DictionaryTypeCheckerTest>() is not TestOutputLogger<DictionaryTypeCheckerTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

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

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaSet = new RecordSchemaSet(loadResult);
        var recordSchemaCatalog = new RecordSchemaCatalog(recordSchemaSet);
        RecordComplianceChecker.Check(recordSchemaCatalog, logger);

        var recordSchema = recordSchemaCatalog.StaticDataRecordSchemata[0];
        foreach (var parameterSchema in recordSchema.RecordPropertySchemata)
        {
            DictionaryTypeChecker.Check(parameterSchema, recordSchemaCatalog, [], logger);
        }

        Assert.Empty(logger.Logs);
    }

    [Fact]
    public void NullableDictionaryNotSupportedTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<DictionaryTypeCheckerTest>() is not TestOutputLogger<DictionaryTypeCheckerTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = @"
            public sealed record Student(
                [Key] string Name,
                int Age
            );

            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyRecord(
                Dictionary<string, Student>? Students,
            );";

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaSet = new RecordSchemaSet(loadResult);
        var recordSchemaCatalog = new RecordSchemaCatalog(recordSchemaSet);

        Assert.Throws<TypeNotSupportedException>(() => RecordComplianceChecker.Check(recordSchemaCatalog, logger));
        Assert.Single(logger.Logs);
    }

    [Fact]
    public void PrimitiveValueDictionaryNotSupportedTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<DictionaryTypeCheckerTest>() is not TestOutputLogger<DictionaryTypeCheckerTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = @"
            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyRecord(
                Dictionary<int, int> Values,
            );";

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaSet = new RecordSchemaSet(loadResult);
        var recordSchemaCatalog = new RecordSchemaCatalog(recordSchemaSet);

        Assert.Throws<TypeNotSupportedException>(() => RecordComplianceChecker.Check(recordSchemaCatalog, logger));
        Assert.Single(logger.Logs);
    }

    [Fact]
    public void NullableKeyDictionaryNotSupportedTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<DictionaryTypeCheckerTest>() is not TestOutputLogger<DictionaryTypeCheckerTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = @"
            public sealed record Student(
                [Key] string Name,
                int Age
            );

            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyRecord(
                Dictionary<string?, Student> Students,
            );";

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaSet = new RecordSchemaSet(loadResult);
        var recordSchemaCatalog = new RecordSchemaCatalog(recordSchemaSet);

        Assert.Throws<TypeNotSupportedException>(() => RecordComplianceChecker.Check(recordSchemaCatalog, logger));
        Assert.Single(logger.Logs);
    }

    [Fact]
    public void NullableRecordValueDictionaryNotSupportedTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<DictionaryTypeCheckerTest>() is not TestOutputLogger<DictionaryTypeCheckerTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = @"
            public sealed record Student(
                [Key] string Name,
                int Age
            );

            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyRecord(
                Dictionary<string, Student?> Students,
            );";

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaSet = new RecordSchemaSet(loadResult);
        var recordSchemaCatalog = new RecordSchemaCatalog(recordSchemaSet);

        Assert.Throws<TypeNotSupportedException>(() => RecordComplianceChecker.Check(recordSchemaCatalog, logger));
        Assert.Single(logger.Logs);
    }

    [Fact]
    public void RecordWithoutKeyDictionaryNotSupportedTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<DictionaryTypeCheckerTest>() is not TestOutputLogger<DictionaryTypeCheckerTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = @"
            public sealed record Student(
                string Name,
                int Age
            );

            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyRecord(
                Dictionary<string, Student> Students,
            );";

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaSet = new RecordSchemaSet(loadResult);
        var recordSchemaCatalog = new RecordSchemaCatalog(recordSchemaSet);

        Assert.Throws<InvalidUsageException>(() => RecordComplianceChecker.Check(recordSchemaCatalog, logger));
        Assert.Single(logger.Logs);
    }

    [Fact]
    public void RecordWithDifferentKeyTypesDictionaryNotSupportedTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<DictionaryTypeCheckerTest>() is not TestOutputLogger<DictionaryTypeCheckerTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = @"
            public sealed record Student(
                [Key] string Name,
                int Age
            );

            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyRecord(
                Dictionary<int, Student> Students,
            );";

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaSet = new RecordSchemaSet(loadResult);
        var recordSchemaCatalog = new RecordSchemaCatalog(recordSchemaSet);

        Assert.Throws<TypeNotSupportedException>(() => RecordComplianceChecker.Check(recordSchemaCatalog, logger));
        Assert.Single(logger.Logs);
    }

    [Fact]
    public void NestedContainerKeyDictionaryNotSupportedTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<DictionaryTypeCheckerTest>() is not TestOutputLogger<DictionaryTypeCheckerTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = @"
            public sealed record NestedRecord(
                [Key] List<int> Values,
                string Data,
            );

            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyRecord(
                Dictionary<List<int>, NestedRecord> Records,
            );";

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaSet = new RecordSchemaSet(loadResult);
        var recordSchemaCatalog = new RecordSchemaCatalog(recordSchemaSet);

        Assert.Throws<InvalidUsageException>(() => RecordComplianceChecker.Check(recordSchemaCatalog, logger));
        Assert.Single(logger.Logs);
    }

    [Fact]
    public void NestedContainerValueDictionaryNotSupportedTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<DictionaryTypeCheckerTest>() is not TestOutputLogger<DictionaryTypeCheckerTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = @"
            public sealed record DataRecord(
                [Key] int Id,
                string Data,
            );

            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyRecord(
                Dictionary<int, List<DataRecord>> Records,
            );";

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaSet = new RecordSchemaSet(loadResult);
        var recordSchemaCatalog = new RecordSchemaCatalog(recordSchemaSet);

        Assert.Throws<TypeNotSupportedException>(() => RecordComplianceChecker.Check(recordSchemaCatalog, logger));
        Assert.Single(logger.Logs);
    }

    [Fact]
    public void ImmutableDictionaryTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<DictionaryTypeCheckerTest>() is not TestOutputLogger<DictionaryTypeCheckerTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

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

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaSet = new RecordSchemaSet(loadResult);
        var recordSchemaCatalog = new RecordSchemaCatalog(recordSchemaSet);
        RecordComplianceChecker.Check(recordSchemaCatalog, logger);

        var recordSchema = recordSchemaCatalog.FindAll("MyRecord").Single();
        foreach (var parameterSchema in recordSchema.RecordPropertySchemata)
        {
            DictionaryTypeChecker.Check(parameterSchema, recordSchemaCatalog, [], logger);
        }

        Assert.Empty(logger.Logs);
    }
}
