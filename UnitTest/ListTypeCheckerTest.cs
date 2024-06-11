using System.Collections.Immutable;
using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner;
using SchemaInfoScanner.Collectors;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.Exceptions;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.TypeCheckers;
using Xunit.Abstractions;

namespace UnitTest;

public class ListTypeCheckerTest
{
    private readonly ITestOutputHelper testOutputHelper;

    public ListTypeCheckerTest(ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void PrimitiveListTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            public enum MyEnum { A, B, C, }
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

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary.Values.First();

        foreach (var parameterSchema in recordSchema.RecordParameterSchemaList)
        {
            ListTypeChecker.Check(parameterSchema, recordSchemaContainer, new(), logger);
        }
    }

    [Fact]
    public void NullablePrimitiveListTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            public enum MyEnum { A, B, C, }
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

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary.Values.First();

        foreach (var parameterSchema in recordSchema.RecordParameterSchemaList)
        {
            ListTypeChecker.Check(parameterSchema, recordSchemaContainer, new(), logger);
        }
    }

    [Fact]
    public void NullableListNotSupportedTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            public sealed record MyRecord(
                List<int>? Values
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        var recordName = new RecordName(".MyRecord");
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary[recordName];

        foreach (var parameterSchema in recordSchema.RecordParameterSchemaList)
        {
            Assert.Throws<TypeNotSupportedException>(() =>
                ListTypeChecker.Check(parameterSchema, recordSchemaContainer, new(), logger));
        }
    }

    [Fact]
    public void NullableRecordListNotSupportedTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            public sealed record Student(string Name, int Age);
            public sealed record MyRecord(
                List<Student?> Students
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        var recordName = new RecordName(".MyRecord");
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary[recordName];

        foreach (var parameterSchema in recordSchema.RecordParameterSchemaList)
        {
            Assert.Throws<TypeNotSupportedException>(() =>
                ListTypeChecker.Check(parameterSchema, recordSchemaContainer, new(), logger));
        }
    }

    [Fact]
    public void NestedContainerListNotSupportedTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            public sealed record MyRecord(
                List<List<int>> Nested,
                List<SortedSet<int>> SortedSets
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        var recordName = new RecordName(".MyRecord");
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary[recordName];

        foreach (var parameterSchema in recordSchema.RecordParameterSchemaList)
        {
            Assert.Throws<TypeNotSupportedException>(() =>
                ListTypeChecker.Check(parameterSchema, recordSchemaContainer, new(), logger));
        }
    }

    [Fact]
    public void SingleColumnPrimitiveContainerNotSupportedTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            public sealed record MyRecord(
                [SingleColumnContainer("", "")] List<int> Values
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        var recordName = new RecordName(".MyRecord");
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary[recordName];

        foreach (var parameterSchema in recordSchema.RecordParameterSchemaList)
        {
            ListTypeChecker.Check(parameterSchema, recordSchemaContainer, new(), logger);
        }
    }

    [Fact]
    public void SingleColumnPrimitiveContainerWithColumnPrefixNotSupportedTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            public sealed record MyRecord(
                [SingleColumnContainer("", "")][ColumnPrefix(""Num_"")] List<int> Values
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        var recordName = new RecordName(".MyRecord");
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary[recordName];

        foreach (var parameterSchema in recordSchema.RecordParameterSchemaList)
        {
            Assert.Throws<InvalidUsageException>(() =>
                ListTypeChecker.Check(parameterSchema, recordSchemaContainer, new(), logger));
        }
    }

    [Fact]
    public void SingleColumnRecordContainerNotSupportedTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            public sealed record Student(string Name, int Age);
            public sealed record MyRecord(
                [SingleColumnContainer("", "")] List<Student> Students
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        var recordName = new RecordName(".MyRecord");
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary[recordName];

        foreach (var parameterSchema in recordSchema.RecordParameterSchemaList)
        {
            Assert.Throws<TypeNotSupportedException>(() =>
                ListTypeChecker.Check(parameterSchema, recordSchemaContainer, new(), logger));
        }
    }

    [Fact]
    public void ImmutableListTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            public sealed record MyRecord(
                List<int> ValuesA,
                ImmutableList<int> ValuesB,
                ImmutableArray<int> ValuesC,
                SortedList<int> ValuesD,
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        var recordName = new RecordName(".MyRecord");
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary[recordName];

        foreach (var parameterSchema in recordSchema.RecordParameterSchemaList)
        {
            ListTypeChecker.Check(parameterSchema, recordSchemaContainer, new(), logger);
        }
    }
}
