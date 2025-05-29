using Microsoft.Extensions.Logging;
using SchemaInfoScanner;
using SchemaInfoScanner.Catalogs;
using SchemaInfoScanner.Collectors;
using SchemaInfoScanner.Exceptions;
using SchemaInfoScanner.TypeCheckers;
using UnitTest.Utility;
using Xunit.Abstractions;

namespace UnitTest;

public class ListTypeCheckerTest(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void PrimitiveListTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<ListTypeCheckerTest>() is not TestOutputLogger<ListTypeCheckerTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = @"
            public enum MyEnum { A, B, C, }

            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyRecord(
                List<bool> BoolValues,
                List<char> CharValues,
                List<sbyte> SByteValues,
                List<byte> ByteValues,
                List<short> ShortValues,
                List<ushort> UShortValues,
                List<int> IntValues,
                List<uint> UIntValues,
                List<long> LongValues,
                List<ulong> ULongValues,
                List<float> FloatValues,
                List<double> DoubleValues,
                List<decimal> DecimalValues,
                List<string> StringValues,
                List<MyEnum> EnumValues
            );";

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaSet = new RecordSchemaSet(loadResult, logger);
        var recordSchemaCatalog = new RecordSchemaCatalog(recordSchemaSet);
        RecordComplianceChecker.Check(recordSchemaCatalog, logger);

        var recordSchema = recordSchemaCatalog.StaticDataRecordSchemata[0];
        foreach (var parameterSchema in recordSchema.RecordPropertySchemata)
        {
            ListTypeChecker.Check(parameterSchema, recordSchemaCatalog, [], logger);
        }

        Assert.Empty(logger.Logs);
    }

    [Fact]
    public void NullablePrimitiveListTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<ListTypeCheckerTest>() is not TestOutputLogger<ListTypeCheckerTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = @"
            public enum MyEnum { A, B, C, }

            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyRecord(
                List<bool?> BoolValues,
                List<char?> CharValues,
                List<sbyte?> SByteValues,
                List<byte?> ByteValues,
                List<short?> ShortValues,
                List<ushort?> UShortValues,
                List<int?> IntValues,
                List<uint?> UIntValues,
                List<long?> LongValues,
                List<ulong?> ULongValues,
                List<float?> FloatValues,
                List<double?> DoubleValues,
                List<decimal?> DecimalValues,
                List<string?> StringValues,
                List<MyEnum?> EnumValues
            );";

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaSet = new RecordSchemaSet(loadResult, logger);
        var recordSchemaCatalog = new RecordSchemaCatalog(recordSchemaSet);
        RecordComplianceChecker.Check(recordSchemaCatalog, logger);

        var recordSchema = recordSchemaCatalog.StaticDataRecordSchemata[0];
        foreach (var parameterSchema in recordSchema.RecordPropertySchemata)
        {
            ListTypeChecker.Check(parameterSchema, recordSchemaCatalog, [], logger);
        }

        Assert.Empty(logger.Logs);
    }

    [Fact]
    public void NullableListNotSupportedTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<ListTypeCheckerTest>() is not TestOutputLogger<ListTypeCheckerTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = @"
            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyRecord(
                List<int>? Values
            );";

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaSet = new RecordSchemaSet(loadResult, logger);
        var enumMemberCatalog = new EnumMemberCatalog(loadResult);
        var recordSchemaCatalog = new RecordSchemaCatalog(recordSchemaSet);

        Assert.Throws<TypeNotSupportedException>(() => RecordComplianceChecker.Check(recordSchemaCatalog, logger));
        Assert.Single(logger.Logs);
    }

    [Fact]
    public void RecordListTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<ListTypeCheckerTest>() is not TestOutputLogger<ListTypeCheckerTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = @"
            public sealed record Student(string Name, int Age);

            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyRecord(
                List<Student> Students
            );";

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaSet = new RecordSchemaSet(loadResult, logger);
        var recordSchemaCatalog = new RecordSchemaCatalog(recordSchemaSet);
        RecordComplianceChecker.Check(recordSchemaCatalog, logger);

        var recordSchema = recordSchemaCatalog.StaticDataRecordSchemata[0];
        foreach (var parameterSchema in recordSchema.RecordPropertySchemata)
        {
            ListTypeChecker.Check(parameterSchema, recordSchemaCatalog, [], logger);
        }

        Assert.Empty(logger.Logs);
    }

    [Fact]
    public void NullableRecordListNotSupportedTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<ListTypeCheckerTest>() is not TestOutputLogger<ListTypeCheckerTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = @"
            public sealed record Student(string Name, int Age);

            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyRecord(
                List<Student?> Students
            );";

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaSet = new RecordSchemaSet(loadResult, logger);
        var enumMemberCatalog = new EnumMemberCatalog(loadResult);
        var recordSchemaCatalog = new RecordSchemaCatalog(recordSchemaSet);

        Assert.Throws<TypeNotSupportedException>(() => RecordComplianceChecker.Check(recordSchemaCatalog, logger));
        Assert.Single(logger.Logs);
    }

    [Fact]
    public void NestedCollectionListNotSupportedTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<ListTypeCheckerTest>() is not TestOutputLogger<ListTypeCheckerTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = @"
            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyRecord(
                List<List<int>> Nested,
                List<SortedSet<int>> SortedSets
            );";

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaSet = new RecordSchemaSet(loadResult, logger);
        var enumMemberCatalog = new EnumMemberCatalog(loadResult);
        var recordSchemaCatalog = new RecordSchemaCatalog(recordSchemaSet);

        Assert.Throws<TypeNotSupportedException>(() => RecordComplianceChecker.Check(recordSchemaCatalog, logger));
        Assert.Single(logger.Logs);
    }

    [Fact]
    public void SingleColumnPrimitiveCollectionTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<ListTypeCheckerTest>() is not TestOutputLogger<ListTypeCheckerTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = @"
            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyRecord(
                [SingleColumnCollection("", "")] List<int> Values
            );";

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaSet = new RecordSchemaSet(loadResult, logger);
        var recordSchemaCatalog = new RecordSchemaCatalog(recordSchemaSet);
        RecordComplianceChecker.Check(recordSchemaCatalog, logger);

        var recordSchema = recordSchemaCatalog.StaticDataRecordSchemata[0];
        foreach (var parameterSchema in recordSchema.RecordPropertySchemata)
        {
            ListTypeChecker.Check(parameterSchema, recordSchemaCatalog, [], logger);
        }

        Assert.Empty(logger.Logs);
    }

    [Fact]
    public void SingleColumnRecordCollectionNotSupportedTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<ListTypeCheckerTest>() is not TestOutputLogger<ListTypeCheckerTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = @"
            public sealed record Student(string Name, int Age);

            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyRecord(
                [SingleColumnCollection("", "")] List<Student> Students
            );";

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaSet = new RecordSchemaSet(loadResult, logger);
        var enumMemberCatalog = new EnumMemberCatalog(loadResult);
        var recordSchemaCatalog = new RecordSchemaCatalog(recordSchemaSet);

        Assert.Throws<TypeNotSupportedException>(() => RecordComplianceChecker.Check(recordSchemaCatalog, logger));
        Assert.Single(logger.Logs);
    }

    [Fact]
    public void ImmutableListTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<ListTypeCheckerTest>() is not TestOutputLogger<ListTypeCheckerTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = @"
            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyRecord(
                List<int> ValuesA,
                ImmutableList<int> ValuesB,
                ImmutableArray<int> ValuesC,
                SortedList<int> ValuesD,
            );";

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaSet = new RecordSchemaSet(loadResult, logger);
        var recordSchemaCatalog = new RecordSchemaCatalog(recordSchemaSet);
        RecordComplianceChecker.Check(recordSchemaCatalog, logger);

        var recordSchema = recordSchemaCatalog.StaticDataRecordSchemata[0];
        foreach (var parameterSchema in recordSchema.RecordPropertySchemata)
        {
            ListTypeChecker.Check(parameterSchema, recordSchemaCatalog, [], logger);
        }

        Assert.Empty(logger.Logs);
    }
}
