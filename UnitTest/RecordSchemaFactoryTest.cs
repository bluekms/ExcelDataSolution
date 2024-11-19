using Microsoft.Extensions.Logging;
using SchemaInfoScanner;
using SchemaInfoScanner.Collectors;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.Schemata;
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
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = $$"""
                     [StaticDataRecord("TestExcel", "TestSheet")]
                     public sealed record MyRecord(
                        {{type}} Parameter
                     );
                     """;

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var enumMemberContainer = new EnumMemberContainer(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector, enumMemberContainer);
        RecordComplianceChecker.Check(recordSchemaContainer, logger);

        var rawRecordSchema = recordSchemaContainer.RecordSchemaDictionary.Values.First();
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

            parameter.CheckCompatibility(valueStr, logger);
        }
    }

    [Theory]
    [InlineData("MyEnum")]
    [InlineData("MyEnum?")]
    public void EnumTypeTest(string type)
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

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
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector, enumMemberContainer);
        RecordComplianceChecker.Check(recordSchemaContainer, logger);

        var rawRecordSchema = recordSchemaContainer.RecordSchemaDictionary.Values.First();
        var recordSchema = RecordSchemaFactory.Create(
            rawRecordSchema,
            recordSchemaContainer,
            enumMemberContainer,
            new Dictionary<string, int>());

        var valueStr = "A";

        var parameter = recordSchema.RecordParameterSchemaList[0];
        parameter.CheckCompatibility(valueStr, logger);
        if (type.EndsWith('?'))
        {
            parameter.CheckCompatibility(string.Empty, logger);
        }
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
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

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
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector, enumMemberContainer);
        RecordComplianceChecker.Check(recordSchemaContainer, logger);

        var rawRecordSchema = recordSchemaContainer.RecordSchemaDictionary.Values.First();
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
        parameter.CheckCompatibility(argument, logger);
    }
}
