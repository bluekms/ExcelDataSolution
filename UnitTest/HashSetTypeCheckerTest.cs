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

        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary.Values.First();

        RecordComplianceChecker.Check(recordSchemaContainer, logger);

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

        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        RecordComplianceChecker.Check(recordSchemaContainer, logger);

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
            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyRecord(
                HashSet<int>? Values,
            );";

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        Assert.Throws<TypeNotSupportedException>(() => RecordComplianceChecker.Check(recordSchemaContainer, logger));
    }

    [Fact]
    public void RecordHashSetTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            public sealed record Student(string Name, int Age);

            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyRecord(
                HashSet<Student> Students,
            );";

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        RecordComplianceChecker.Check(recordSchemaContainer, logger);

        var recordName = new RecordName("MyRecord");
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary[recordName];
        foreach (var parameterSchema in recordSchema.RecordParameterSchemaList)
        {
            HashSetTypeChecker.Check(parameterSchema, recordSchemaContainer, new(), logger);
        }
    }

    [Fact]
    public void NullableRecordHashSetNotSupportedTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            public sealed record Student(string Name, int Age);

            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyRecord(
                HashSet<Student?> Students,
            );";

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        Assert.Throws<TypeNotSupportedException>(() => RecordComplianceChecker.Check(recordSchemaContainer, logger));
    }

    [Fact]
    public void NestedContainerHashSetNotSupportedTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyRecord(
                HashSet<HashSet<int>> Nested,
                HashSet<SortedSet<int>> SortedSets,
            );";

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        Assert.Throws<TypeNotSupportedException>(() => RecordComplianceChecker.Check(recordSchemaContainer, logger));
    }

    [Fact]
    public void SingleColumnPrimitiveContainerNotSupportedTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyRecord(
                [SingleColumnContainer("", "")] HashSet<int> Values
            );";

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        RecordComplianceChecker.Check(recordSchemaContainer, logger);

        var recordName = new RecordName("MyRecord");
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary[recordName];
        foreach (var parameterSchema in recordSchema.RecordParameterSchemaList)
        {
            HashSetTypeChecker.Check(parameterSchema, recordSchemaContainer, new(), logger);
        }
    }

    [Fact]
    public void SingleColumnRecordContainerNotSupportedTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            public sealed record Student(string Name, int Age);

            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyRecord(
                [SingleColumnContainer("", "")] HashSet<Student> Students
            );";

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        Assert.Throws<TypeNotSupportedException>(() => RecordComplianceChecker.Check(recordSchemaContainer, logger));
    }

    [Fact]
    public void ImmutableHashSetTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyRecord(
                HashSet<int> ValuesA,
                ImmutableHashSet<int> ValuesB,
                ImmutableSortedSet<int> ValuesC
            );";

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        RecordComplianceChecker.Check(recordSchemaContainer, logger);

        var recordName = new RecordName("MyRecord");
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary[recordName];
        foreach (var parameterSchema in recordSchema.RecordParameterSchemaList)
        {
            HashSetTypeChecker.Check(parameterSchema, recordSchemaContainer, new(), logger);
        }
    }
}
