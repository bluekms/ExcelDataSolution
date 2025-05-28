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
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<PrimitiveTypeCheckerTest>() is not TestOutputLogger<PrimitiveTypeCheckerTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = """
                   public enum MyEnum { A, B, C, }

                   [StaticDataRecord("Test", "TestSheet")]
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
                   );
                   """;

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaSet = new RecordSchemaSet(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaSet);
        RecordComplianceChecker.Check(recordSchemaContainer, logger);

        var recordSchema = recordSchemaContainer.StaticDataRecordSchemata[0];
        foreach (var parameterSchema in recordSchema.RecordParameterSchemaList)
        {
            PrimitiveTypeChecker.Check(parameterSchema);
        }

        Assert.Empty(logger.Logs);
    }

    [Fact]
    public void NullableTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<PrimitiveTypeCheckerTest>() is not TestOutputLogger<PrimitiveTypeCheckerTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = """
                   public enum MyEnum { A, B, C, }

                   [StaticDataRecord("Test", "TestSheet")]
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
                   );
                   """;

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaSet = new RecordSchemaSet(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaSet);
        RecordComplianceChecker.Check(recordSchemaContainer, logger);

        var recordSchema = recordSchemaContainer.StaticDataRecordSchemata[0];
        foreach (var parameterSchema in recordSchema.RecordParameterSchemaList)
        {
            PrimitiveTypeChecker.Check(parameterSchema);
        }

        Assert.Empty(logger.Logs);
    }

    [Fact]
    public void NullableAttributeTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<PrimitiveTypeCheckerTest>() is not TestOutputLogger<PrimitiveTypeCheckerTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = """
                   [StaticDataRecord("Test", "TestSheet")]
                   public sealed record MyRecord(
                       [NullString("*")] int? nullableValue,
                       [NullString("*")] int notnullValue,
                   );
                   """;

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaSet = new RecordSchemaSet(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaSet);
        Assert.Throws<InvalidUsageException>(() => RecordComplianceChecker.Check(recordSchemaContainer, logger));

        var recordSchema = recordSchemaContainer.StaticDataRecordSchemata[0];
        var nullableParameter = recordSchema.RecordParameterSchemaList[0];
        var notnullParameter = recordSchema.RecordParameterSchemaList[1];

        PrimitiveTypeChecker.Check(nullableParameter);
        Assert.Throws<InvalidUsageException>(() => PrimitiveTypeChecker.Check(notnullParameter));
        Assert.Single(logger.Logs);
    }
}
