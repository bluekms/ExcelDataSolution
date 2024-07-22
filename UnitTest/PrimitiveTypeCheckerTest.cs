using Microsoft.Extensions.Logging;
using SchemaInfoScanner;
using SchemaInfoScanner.Collectors;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.Exceptions;
using SchemaInfoScanner.TypeCheckers;
using Xunit.Abstractions;

namespace UnitTest;

public class PrimitiveTypeCheckerTest
{
    private readonly ITestOutputHelper testOutputHelper;

    public PrimitiveTypeCheckerTest(ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void Test()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            public enum MyEnum { A, B, C, }
            public sealed record MyRecord(
                bool BoolValue,
                char CharValue,
                sbyte SByteValue,
                byte ByteValue,
                short ShortValue,
                ushort UShortValue,
                int IntValue,
                uint UIntValue,
                long LongValue,
                ulong ULongValue,
                float FloatValue,
                double DoubleValue,
                decimal DecimalValue,
                string StringValue,
                MyEnum EnumValue,
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary.Values.First();

        foreach (var parameterSchema in recordSchema.RecordParameterSchemaList)
        {
            PrimitiveTypeChecker.Check(parameterSchema);
        }
    }

    [Fact]
    public void NullableTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            public enum MyEnum { A, B, C, }
            public sealed record MyRecord(
                bool? BoolValue,
                char? CharValue,
                sbyte? SByteValue,
                byte? ByteValue,
                short? ShortValue,
                ushort? UShortValue,
                int? IntValue,
                uint? UIntValue,
                long? LongValue,
                ulong? ULongValue,
                float? FloatValue,
                double? DoubleValue,
                decimal? DecimalValue,
                string? StringValue,
                MyEnum? EnumValue,
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary.Values.First();

        foreach (var parameterSchema in recordSchema.RecordParameterSchemaList)
        {
            PrimitiveTypeChecker.Check(parameterSchema);
        }
    }

    [Fact]
    public void NullableAttributeTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            public sealed record MyRecord(
                [NullString(""*"")] int? nullableValue,
                [NullString(""*"")] int notnullValue,
            );";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary.Values.First();
        var nullableParameter = recordSchema.RecordParameterSchemaList[0];
        var notnullParameter = recordSchema.RecordParameterSchemaList[1];

        PrimitiveTypeChecker.Check(nullableParameter);
        Assert.Throws<InvalidUsageException>(() => PrimitiveTypeChecker.Check(notnullParameter));
    }
}
