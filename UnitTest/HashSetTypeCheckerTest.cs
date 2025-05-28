using Microsoft.Extensions.Logging;
using SchemaInfoScanner;
using SchemaInfoScanner.Collectors;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.Exceptions;
using SchemaInfoScanner.TypeCheckers;
using UnitTest.Utility;
using Xunit.Abstractions;

namespace UnitTest;

public class HashSetTypeCheckerTest(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void PrimitiveHashSetTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<HashSetTypeCheckerTest>() is not TestOutputLogger<HashSetTypeCheckerTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = @"
            public enum MyEnum { A, B, C, }

            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyRecord(
                HashSet<bool> BoolValues,
                HashSet<char> CharValues,
                HashSet<sbyte> SByteValues,
                HashSet<byte> ByteValues,
                HashSet<short> ShortValues,
                HashSet<ushort> UShortValues,
                HashSet<int> IntValues,
                HashSet<uint> UIntValues,
                HashSet<long> LongValues,
                HashSet<ulong> ULongValues,
                HashSet<float> FloatValues,
                HashSet<double> DoubleValues,
                HashSet<decimal> DecimalValues,
                HashSet<string> StringValues,
                HashSet<MyEnum> EnumValues,
            );";

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaSet = new RecordSchemaSet(loadResult);
        var recordSchemaCatalog = new RecordSchemaCatalog(recordSchemaSet);

        RecordComplianceChecker.Check(recordSchemaCatalog, logger);

        var recordSchema = recordSchemaCatalog.StaticDataRecordSchemata[0];
        foreach (var parameterSchema in recordSchema.RecordParameterSchemaList)
        {
            HashSetTypeChecker.Check(parameterSchema, recordSchemaCatalog, [], logger);
        }

        Assert.Empty(logger.Logs);
    }

    [Fact]
    public void NullablePrimitiveHashSetTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<HashSetTypeCheckerTest>() is not TestOutputLogger<HashSetTypeCheckerTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = @"
            public enum MyEnum { A, B, C, }

            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyRecord(
                HashSet<bool?> BoolValues,
                HashSet<char?> CharValues,
                HashSet<sbyte?> SByteValues,
                HashSet<byte?> ByteValues,
                HashSet<short?> ShortValues,
                HashSet<ushort?> UShortValues,
                HashSet<int?> IntValues,
                HashSet<uint?> UIntValues,
                HashSet<long?> LongValues,
                HashSet<ulong?> ULongValues,
                HashSet<float?> FloatValues,
                HashSet<double?> DoubleValues,
                HashSet<decimal?> DecimalValues,
                HashSet<string?> StringValues,
                HashSet<MyEnum?> EnumValues,
            );";

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaSet = new RecordSchemaSet(loadResult);
        var recordSchemaCatalog = new RecordSchemaCatalog(recordSchemaSet);

        RecordComplianceChecker.Check(recordSchemaCatalog, logger);

        var recordSchema = recordSchemaCatalog.StaticDataRecordSchemata[0];
        foreach (var parameterSchema in recordSchema.RecordParameterSchemaList)
        {
            HashSetTypeChecker.Check(parameterSchema, recordSchemaCatalog, [], logger);
        }

        Assert.Empty(logger.Logs);
    }

    [Fact]
    public void NullableHashSetNotSupportedTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<HashSetTypeCheckerTest>() is not TestOutputLogger<HashSetTypeCheckerTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = @"
            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyRecord(
                HashSet<int>? Values,
            );";

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaSet = new RecordSchemaSet(loadResult);
        var enumMemberContainer = new EnumMemberContainer(loadResult);
        var recordSchemaCatalog = new RecordSchemaCatalog(recordSchemaSet);

        Assert.Throws<TypeNotSupportedException>(() => RecordComplianceChecker.Check(recordSchemaCatalog, logger));
        Assert.Single(logger.Logs);
    }

    [Fact]
    public void RecordHashSetTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<HashSetTypeCheckerTest>() is not TestOutputLogger<HashSetTypeCheckerTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = @"
            public sealed record Student(string Name, int Age);

            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyRecord(
                HashSet<Student> Students,
            );";

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaSet = new RecordSchemaSet(loadResult);
        var recordSchemaCatalog = new RecordSchemaCatalog(recordSchemaSet);

        RecordComplianceChecker.Check(recordSchemaCatalog, logger);

        var recordSchema = recordSchemaCatalog.StaticDataRecordSchemata[0];
        foreach (var parameterSchema in recordSchema.RecordParameterSchemaList)
        {
            HashSetTypeChecker.Check(parameterSchema, recordSchemaCatalog, [], logger);
        }

        Assert.Empty(logger.Logs);
    }

    [Fact]
    public void NullableRecordHashSetNotSupportedTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<HashSetTypeCheckerTest>() is not TestOutputLogger<HashSetTypeCheckerTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = @"
            public sealed record Student(string Name, int Age);

            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyRecord(
                HashSet<Student?> Students,
            );";

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaSet = new RecordSchemaSet(loadResult);
        var enumMemberContainer = new EnumMemberContainer(loadResult);
        var recordSchemaCatalog = new RecordSchemaCatalog(recordSchemaSet);

        Assert.Throws<TypeNotSupportedException>(() => RecordComplianceChecker.Check(recordSchemaCatalog, logger));
        Assert.Single(logger.Logs);
    }

    [Fact]
    public void NestedContainerHashSetNotSupportedTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<HashSetTypeCheckerTest>() is not TestOutputLogger<HashSetTypeCheckerTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = @"
            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyRecord(
                HashSet<HashSet<int>> Nested,
                HashSet<SortedSet<int>> SortedSets,
            );";

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaSet = new RecordSchemaSet(loadResult);
        var enumMemberContainer = new EnumMemberContainer(loadResult);
        var recordSchemaCatalog = new RecordSchemaCatalog(recordSchemaSet);

        Assert.Throws<TypeNotSupportedException>(() => RecordComplianceChecker.Check(recordSchemaCatalog, logger));
        Assert.Single(logger.Logs);
    }

    [Fact]
    public void SingleColumnPrimitiveContainerNotSupportedTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<HashSetTypeCheckerTest>() is not TestOutputLogger<HashSetTypeCheckerTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = @"
            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyRecord(
                [SingleColumnContainer("", "")] HashSet<int> Values
            );";

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaSet = new RecordSchemaSet(loadResult);
        var recordSchemaCatalog = new RecordSchemaCatalog(recordSchemaSet);
        RecordComplianceChecker.Check(recordSchemaCatalog, logger);

        var recordSchema = recordSchemaCatalog.StaticDataRecordSchemata[0];
        foreach (var parameterSchema in recordSchema.RecordParameterSchemaList)
        {
            HashSetTypeChecker.Check(parameterSchema, recordSchemaCatalog, [], logger);
        }

        Assert.Empty(logger.Logs);
    }

    [Fact]
    public void SingleColumnRecordContainerNotSupportedTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<HashSetTypeCheckerTest>() is not TestOutputLogger<HashSetTypeCheckerTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = @"
            public sealed record Student(string Name, int Age);

            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyRecord(
                [SingleColumnContainer("", "")] HashSet<Student> Students
            );";

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaSet = new RecordSchemaSet(loadResult);
        var enumMemberContainer = new EnumMemberContainer(loadResult);
        var recordSchemaCatalog = new RecordSchemaCatalog(recordSchemaSet);

        Assert.Throws<TypeNotSupportedException>(() => RecordComplianceChecker.Check(recordSchemaCatalog, logger));
        Assert.Single(logger.Logs);
    }

    [Fact]
    public void ImmutableHashSetTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<HashSetTypeCheckerTest>() is not TestOutputLogger<HashSetTypeCheckerTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = @"
            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyRecord(
                HashSet<int> ValuesA,
                ImmutableHashSet<int> ValuesB,
                ImmutableSortedSet<int> ValuesC
            );";

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaSet = new RecordSchemaSet(loadResult);
        var recordSchemaCatalog = new RecordSchemaCatalog(recordSchemaSet);
        RecordComplianceChecker.Check(recordSchemaCatalog, logger);

        var recordSchema = recordSchemaCatalog.StaticDataRecordSchemata[0];
        foreach (var parameterSchema in recordSchema.RecordParameterSchemaList)
        {
            HashSetTypeChecker.Check(parameterSchema, recordSchemaCatalog, [], logger);
        }

        Assert.Empty(logger.Logs);
    }
}
