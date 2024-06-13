using Microsoft.Extensions.Logging;
using SchemaInfoScanner;
using SchemaInfoScanner.Collectors;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.Exceptions;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.TypeCheckers;
using Xunit.Abstractions;

namespace UnitTest;

public class HashSetTypeCheckerTest
{
    private readonly ITestOutputHelper testOutputHelper;

    public HashSetTypeCheckerTest(ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void PrimitiveHashSetTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            public enum MyEnum { A, B, C, }
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

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary.Values.First();

        foreach (var parameterSchema in recordSchema.RecordParameterSchemaList)
        {
            HashSetTypeChecker.Check(parameterSchema, recordSchemaContainer, new(), logger);
        }
    }

    [Fact]
    public void NullablePrimitiveHashSetTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            public enum MyEnum { A, B, C, }
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

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary.Values.First();

        foreach (var parameterSchema in recordSchema.RecordParameterSchemaList)
        {
            HashSetTypeChecker.Check(parameterSchema, recordSchemaContainer, new(), logger);
        }
    }

    [Fact]
    public void NullableHashSetNotSupportedTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            public sealed record MyRecord(
                HashSet<int>? Values,
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        var recordName = new RecordName(".MyRecord");
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary[recordName];

        foreach (var parameterSchema in recordSchema.RecordParameterSchemaList)
        {
            Assert.Throws<TypeNotSupportedException>(() =>
                HashSetTypeChecker.Check(parameterSchema, recordSchemaContainer, new(), logger));
        }
    }

    [Fact]
    public void RecordHashSetTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            public sealed record Student(string Name, int Age);
            public sealed record MyRecord(
                HashSet<Student> Students,
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        var recordName = new RecordName(".MyRecord");
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary[recordName];

        foreach (var parameterSchema in recordSchema.RecordParameterSchemaList)
        {
            HashSetTypeChecker.Check(parameterSchema, recordSchemaContainer, new(), logger);
        }
    }

    [Fact]
    public void NullableRecordHashSetTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            public sealed record Student(string Name, int Age);
            public sealed record MyRecord(
                HashSet<Student?> Students,
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        var recordName = new RecordName(".MyRecord");
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary[recordName];

        foreach (var parameterSchema in recordSchema.RecordParameterSchemaList)
        {
            HashSetTypeChecker.Check(parameterSchema, recordSchemaContainer, new(), logger);
        }
    }

    [Fact]
    public void NestedContainerHashSetNotSupportedTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            public sealed record MyRecord(
                HashSet<HashSet<int>> Nested,
                HashSet<SortedSet<int>> SortedSets,
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        var recordName = new RecordName(".MyRecord");
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary[recordName];

        foreach (var parameterSchema in recordSchema.RecordParameterSchemaList)
        {
            Assert.Throws<TypeNotSupportedException>(() =>
                HashSetTypeChecker.Check(parameterSchema, recordSchemaContainer, new(), logger));
        }
    }

    [Fact]
    public void SingleColumnPrimitiveContainerNotSupportedTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            public sealed record MyRecord(
                [SingleColumnContainer("", "")] HashSet<int> Values
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        var recordName = new RecordName(".MyRecord");
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary[recordName];

        foreach (var parameterSchema in recordSchema.RecordParameterSchemaList)
        {
            HashSetTypeChecker.Check(parameterSchema, recordSchemaContainer, new(), logger);
        }
    }

    [Fact]
    public void SingleColumnPrimitiveContainerWithColumnPrefixNotSupportedTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            public sealed record MyRecord(
                [SingleColumnContainer("", "")][ColumnPrefix(""Num_"")] HashSet<int> Values
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        var recordName = new RecordName(".MyRecord");
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary[recordName];

        foreach (var parameterSchema in recordSchema.RecordParameterSchemaList)
        {
            Assert.Throws<InvalidUsageException>(() =>
                HashSetTypeChecker.Check(parameterSchema, recordSchemaContainer, new(), logger));
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
                [SingleColumnContainer("", "")] HashSet<Student> Students
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        var recordName = new RecordName(".MyRecord");
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary[recordName];

        foreach (var parameterSchema in recordSchema.RecordParameterSchemaList)
        {
            Assert.Throws<TypeNotSupportedException>(() =>
                HashSetTypeChecker.Check(parameterSchema, recordSchemaContainer, new(), logger));
        }
    }

    [Fact]
    public void ImmutableHashSetTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            public sealed record MyRecord(
                HashSet<int> ValuesA,
                ImmutableHashSet<int> ValuesB,
                ImmutableSortedSet<int> ValuesC
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        var recordName = new RecordName(".MyRecord");
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary[recordName];

        foreach (var parameterSchema in recordSchema.RecordParameterSchemaList)
        {
            HashSetTypeChecker.Check(parameterSchema, recordSchemaContainer, new(), logger);
        }
    }
}
