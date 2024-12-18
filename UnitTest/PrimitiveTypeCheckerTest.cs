using Microsoft.Extensions.Logging;
using SchemaInfoScanner;
using SchemaInfoScanner.Collectors;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.Exceptions;
using SchemaInfoScanner.TypeCheckers;
using UnitTest.Utility;
using Xunit.Abstractions;

namespace UnitTest;

public class PrimitiveTypeCheckerTest(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void Test()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Trace);
        if (factory.CreateLogger<PrimitiveTypeCheckerTest>() is not TestOutputLogger<PrimitiveTypeCheckerTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

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

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var enumMemberContainer = new EnumMemberContainer(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector, enumMemberContainer);

        var recordSchema = recordSchemaContainer.RecordSchemaDictionary.Values.First();

        foreach (var parameterSchema in recordSchema.RawParameterSchemaList)
        {
            PrimitiveTypeChecker.Check(parameterSchema);
        }

        Assert.Empty(logger.Logs);
    }

    [Fact]
    public void NullableTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Trace);
        if (factory.CreateLogger<PrimitiveTypeCheckerTest>() is not TestOutputLogger<PrimitiveTypeCheckerTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

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

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var enumMemberContainer = new EnumMemberContainer(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector, enumMemberContainer);

        var recordSchema = recordSchemaContainer.RecordSchemaDictionary.Values.First();

        foreach (var parameterSchema in recordSchema.RawParameterSchemaList)
        {
            PrimitiveTypeChecker.Check(parameterSchema);
        }

        Assert.Empty(logger.Logs);
    }

    [Fact]
    public void NullableAttributeTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Trace);
        if (factory.CreateLogger<PrimitiveTypeCheckerTest>() is not TestOutputLogger<PrimitiveTypeCheckerTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = @"
            public sealed record MyRecord(
                [NullString(""*"")] int? nullableValue,
                [NullString(""*"")] int notnullValue,
            );";

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var enumMemberContainer = new EnumMemberContainer(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector, enumMemberContainer);

        var recordSchema = recordSchemaContainer.RecordSchemaDictionary.Values.First();
        var nullableParameter = recordSchema.RawParameterSchemaList[0];
        var notnullParameter = recordSchema.RawParameterSchemaList[1];

        PrimitiveTypeChecker.Check(nullableParameter);
        Assert.Throws<InvalidUsageException>(() => PrimitiveTypeChecker.Check(notnullParameter));
        Assert.Empty(logger.Logs);
    }
}
