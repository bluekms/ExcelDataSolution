using Microsoft.Extensions.Logging;
using SchemaInfoScanner;
using SchemaInfoScanner.Collectors;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.Schemata;
using SchemaInfoScanner.Schemata.TypedParameterSchemata;
using UnitTest.Utility;
using Xunit.Abstractions;

namespace UnitTest;

public class RecordSchemaFactoryTest(ITestOutputHelper testOutputHelper)
{
    [Theory]
    [InlineData("bool")]
    [InlineData("byte")]
    [InlineData("char")]
    [InlineData("decimal")]
    [InlineData("double")]
    [InlineData("float")]
    [InlineData("int")]
    [InlineData("long")]
    [InlineData("sbyte")]
    [InlineData("short")]
    [InlineData("string")]
    [InlineData("uint")]
    [InlineData("ulong")]
    [InlineData("ushort")]
    [InlineData("bool?")]
    [InlineData("byte?")]
    [InlineData("char?")]
    [InlineData("decimal?")]
    [InlineData("double?")]
    [InlineData("float?")]
    [InlineData("int?")]
    [InlineData("long?")]
    [InlineData("sbyte?")]
    [InlineData("short?")]
    [InlineData("string?")]
    [InlineData("uint?")]
    [InlineData("ulong?")]
    [InlineData("ushort?")]
    public void PrimitiveTypeTest(string type)
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<RecordSchemaFactoryTest>() is not TestOutputLogger<RecordSchemaFactoryTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     [StaticDataRecord("TestExcel", "TestSheet")]
                     public sealed record MyRecord(
                        {{type}} Parameter
                     );
                     """;

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var enumMemberContainer = new EnumMemberContainer(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);
        RecordComplianceChecker.Check(recordSchemaContainer, logger);

        var rawRecordSchema = recordSchemaContainer.StaticDataRecordSchemata[0];
        var recordSchema = RecordSchemaFactory.Create(
            rawRecordSchema,
            recordSchemaContainer,
            enumMemberContainer,
            new Dictionary<string, int>());

        for (var i = 0; i < 10; ++i)
        {
            var parameter = recordSchema.RecordParameterSchemaList[0];
            var value = RandomValueGenerator.Generate(TypeConverter.GetSystemTypeName(type));
            var valueStr = value?.ToString() ?? string.Empty;

            parameter.CheckCompatibility(valueStr, enumMemberContainer, logger);
        }

        Assert.Empty(logger.Logs);
    }

    [Theory]
    [InlineData("MyEnum")]
    [InlineData("MyEnum?")]
    public void EnumTypeTest(string type)
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<RecordSchemaFactoryTest>() is not TestOutputLogger<RecordSchemaFactoryTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     public enum MyEnum { A, B, C }

                     [StaticDataRecord("TestExcel", "TestSheet")]
                     public sealed record MyRecord(
                        {{type}} Parameter
                     );
                     """;

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var enumMemberContainer = new EnumMemberContainer(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);
        RecordComplianceChecker.Check(recordSchemaContainer, logger);

        var rawRecordSchema = recordSchemaContainer.StaticDataRecordSchemata[0];
        var recordSchema = RecordSchemaFactory.Create(
            rawRecordSchema,
            recordSchemaContainer,
            enumMemberContainer,
            new Dictionary<string, int>());

        var parameter = recordSchema.RecordParameterSchemaList[0];
        if (parameter is EnumParameterSchema enumParameter)
        {
            enumParameter.CheckCompatibility("A", enumMemberContainer, logger);
        }
        else if (parameter is NullableEnumParameterSchema nullableEnumParameter)
        {
            nullableEnumParameter.CheckCompatibility(string.Empty, enumMemberContainer, logger);
        }
        else
        {
            throw new InvalidOperationException("EnumParameterSchema or NullableEnumParameterSchema is expected.");
        }

        Assert.Empty(logger.Logs);
    }

    [Theory]
    [InlineData("bool")]
    [InlineData("byte")]
    [InlineData("char")]
    [InlineData("decimal")]
    [InlineData("double")]
    [InlineData("float")]
    [InlineData("int")]
    [InlineData("long")]
    [InlineData("sbyte")]
    [InlineData("short")]
    [InlineData("string")]
    [InlineData("uint")]
    [InlineData("ulong")]
    [InlineData("ushort")]
    [InlineData("bool?")]
    [InlineData("byte?")]
    [InlineData("char?")]
    [InlineData("decimal?")]
    [InlineData("double?")]
    [InlineData("float?")]
    [InlineData("int?")]
    [InlineData("long?")]
    [InlineData("sbyte?")]
    [InlineData("short?")]
    [InlineData("string?")]
    [InlineData("uint?")]
    [InlineData("ulong?")]
    [InlineData("ushort?")]
    public void SingleColumnListTest(string type)
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<RecordSchemaFactoryTest>() is not TestOutputLogger<RecordSchemaFactoryTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     [StaticDataRecord("TestExcel", "TestSheet")]
                     public sealed record MyRecord(
                        [SingleColumnContainer(",")]
                        List<{{type}}> Parameter
                     );
                     """;

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var enumMemberContainer = new EnumMemberContainer(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);
        RecordComplianceChecker.Check(recordSchemaContainer, logger);

        var rawRecordSchema = recordSchemaContainer.StaticDataRecordSchemata[0];
        var recordSchema = RecordSchemaFactory.Create(
            rawRecordSchema,
            recordSchemaContainer,
            enumMemberContainer,
            new Dictionary<string, int>());

        var count = Random.Shared.Next(2, 10);
        var list = Enumerable.Repeat(
            RandomValueGenerator.Generate(TypeConverter.GetSystemTypeName(type))?.ToString() ?? string.Empty,
            count);
        var argument = string.Join(", ", list);

        var parameter = recordSchema.RecordParameterSchemaList[0];
        parameter.CheckCompatibility(argument, enumMemberContainer, logger);

        Assert.Empty(logger.Logs);
    }

    private enum MyEnumForTest
    {
        A,
        B,
        C,
    }

    [Theory]
    [InlineData("List", "MyEnumForTest")]
    [InlineData("List", "MyEnumForTest?")]
    [InlineData("HashSet", "MyEnumForTest")]
    [InlineData("HashSet", "MyEnumForTest?")]
    public void SingleColumnEnumListTest(string container, string type)
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<RecordSchemaFactoryTest>() is not TestOutputLogger<RecordSchemaFactoryTest> logger)
        {
            throw new InvalidOperationException("Logger Type Error.");
        }

        var code = $$"""
                     public enum MyEnumForTest { A, B, C }

                     [StaticDataRecord("TestExcel", "TestSheet")]
                     public sealed record MyRecord(
                        [SingleColumnContainer(",")]
                        {{container}}<{{type}}> Parameter
                     );
                     """;

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var enumMemberContainer = new EnumMemberContainer(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);
        RecordComplianceChecker.Check(recordSchemaContainer, logger);

        var rawRecordSchema = recordSchemaContainer.StaticDataRecordSchemata[0];
        var recordSchema = RecordSchemaFactory.Create(
            rawRecordSchema,
            recordSchemaContainer,
            enumMemberContainer,
            new Dictionary<string, int>());

        const string argument = "A, B, C";

        var parameter = recordSchema.RecordParameterSchemaList[0];
        parameter.CheckCompatibility(argument, enumMemberContainer, logger);

        Assert.Empty(logger.Logs);
    }
}
