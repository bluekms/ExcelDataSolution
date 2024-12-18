using Microsoft.Extensions.Logging;
using SchemaInfoScanner;
using SchemaInfoScanner.Collectors;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.Exceptions;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.TypeCheckers;
using UnitTest.Utility;
using Xunit.Abstractions;

namespace UnitTest;

public class ListTypeCheckerTest(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void PrimitiveListTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Trace);
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

        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var enumMemberContainer = new EnumMemberContainer(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector, enumMemberContainer);

        RecordComplianceChecker.Check(recordSchemaContainer, logger);

        var recordSchema = recordSchemaContainer.RecordSchemaDictionary.Values.First();
        foreach (var parameterSchema in recordSchema.RawParameterSchemaList)
        {
            ListTypeChecker.Check(parameterSchema, recordSchemaContainer, new(), logger);
        }

        Assert.Empty(logger.Logs);
    }

    [Fact]
    public void NullablePrimitiveListTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Trace);
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

        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var enumMemberContainer = new EnumMemberContainer(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector, enumMemberContainer);

        RecordComplianceChecker.Check(recordSchemaContainer, logger);

        var recordSchema = recordSchemaContainer.RecordSchemaDictionary.Values.First();
        foreach (var parameterSchema in recordSchema.RawParameterSchemaList)
        {
            ListTypeChecker.Check(parameterSchema, recordSchemaContainer, new(), logger);
        }

        Assert.Empty(logger.Logs);
    }

    [Fact]
    public void NullableListNotSupportedTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Trace);
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

        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var enumMemberContainer = new EnumMemberContainer(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector, enumMemberContainer);

        Assert.Throws<TypeNotSupportedException>(() => RecordComplianceChecker.Check(recordSchemaContainer, logger));
        Assert.Empty(logger.Logs);
    }

    [Fact]
    public void RecordListTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Trace);
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

        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var enumMemberContainer = new EnumMemberContainer(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector, enumMemberContainer);

        RecordComplianceChecker.Check(recordSchemaContainer, logger);

        var recordName = new RecordName("MyRecord");
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary[recordName];
        foreach (var parameterSchema in recordSchema.RawParameterSchemaList)
        {
            ListTypeChecker.Check(parameterSchema, recordSchemaContainer, new(), logger);
        }

        Assert.Empty(logger.Logs);
    }

    [Fact]
    public void NullableRecordListNotSupportedTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Trace);
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

        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var enumMemberContainer = new EnumMemberContainer(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector, enumMemberContainer);

        Assert.Throws<TypeNotSupportedException>(() => RecordComplianceChecker.Check(recordSchemaContainer, logger));
        Assert.Empty(logger.Logs);
    }

    [Fact]
    public void NestedContainerListNotSupportedTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Trace);
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

        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var enumMemberContainer = new EnumMemberContainer(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector, enumMemberContainer);

        Assert.Throws<TypeNotSupportedException>(() => RecordComplianceChecker.Check(recordSchemaContainer, logger));
        Assert.Empty(logger.Logs);
    }

    [Fact]
    public void SingleColumnPrimitiveContainerTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Trace);
        if (factory.CreateLogger<ListTypeCheckerTest>() is not TestOutputLogger<ListTypeCheckerTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = @"
            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyRecord(
                [SingleColumnContainer("", "")] List<int> Values
            );";

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var enumMemberContainer = new EnumMemberContainer(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector, enumMemberContainer);

        RecordComplianceChecker.Check(recordSchemaContainer, logger);

        var recordName = new RecordName("MyRecord");
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary[recordName];
        foreach (var parameterSchema in recordSchema.RawParameterSchemaList)
        {
            ListTypeChecker.Check(parameterSchema, recordSchemaContainer, new(), logger);
        }

        Assert.Empty(logger.Logs);
    }

    [Fact]
    public void SingleColumnRecordContainerNotSupportedTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Trace);
        if (factory.CreateLogger<ListTypeCheckerTest>() is not TestOutputLogger<ListTypeCheckerTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = @"
            public sealed record Student(string Name, int Age);

            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyRecord(
                [SingleColumnContainer("", "")] List<Student> Students
            );";

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var enumMemberContainer = new EnumMemberContainer(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector, enumMemberContainer);

        Assert.Throws<TypeNotSupportedException>(() => RecordComplianceChecker.Check(recordSchemaContainer, logger));
        Assert.Empty(logger.Logs);
    }

    [Fact]
    public void ImmutableListTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Trace);
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

        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var enumMemberContainer = new EnumMemberContainer(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector, enumMemberContainer);

        RecordComplianceChecker.Check(recordSchemaContainer, logger);

        var recordName = new RecordName("MyRecord");
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary[recordName];
        foreach (var parameterSchema in recordSchema.RawParameterSchemaList)
        {
            ListTypeChecker.Check(parameterSchema, recordSchemaContainer, new(), logger);
        }

        Assert.Empty(logger.Logs);
    }
}
