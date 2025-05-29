using Microsoft.Extensions.Logging;
using SchemaInfoScanner;
using SchemaInfoScanner.Catalogs;
using SchemaInfoScanner.Collectors;
using SchemaInfoScanner.Schemata;
using SchemaInfoScanner.Schemata.TypedPropertySchemata.PrimitiveTypes;
using SchemaInfoScanner.Schemata.TypedPropertySchemata.PrimitiveTypes.NullableTypes;
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

        var recordSchemaSet = new RecordSchemaSet(loadResult, logger);
        var enumMemberCatalog = new EnumMemberCatalog(loadResult);
        var recordSchemaCatalog = new RecordSchemaCatalog(recordSchemaSet);
        RecordComplianceChecker.Check(recordSchemaCatalog, logger);

        var rawRecordSchema = recordSchemaCatalog.StaticDataRecordSchemata[0];
        var recordSchema = RecordSchemaFactory.Create(
            rawRecordSchema,
            recordSchemaCatalog,
            new Dictionary<string, int>());

        for (var i = 0; i < 10; ++i)
        {
            var parameter = recordSchema.RecordPropertySchemata[0];
            var value = RandomValueGenerator.Generate(TypeConverter.GetSystemTypeName(type));

            var valueStr = value?.ToString() ?? string.Empty;
            var arguments = Enumerable.Repeat(valueStr, 1).GetEnumerator();
            parameter.CheckCompatibility(arguments, enumMemberCatalog, logger);
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

        var recordSchemaSet = new RecordSchemaSet(loadResult, logger);
        var enumMemberCatalog = new EnumMemberCatalog(loadResult);
        var recordSchemaCatalog = new RecordSchemaCatalog(recordSchemaSet);
        RecordComplianceChecker.Check(recordSchemaCatalog, logger);

        var rawRecordSchema = recordSchemaCatalog.StaticDataRecordSchemata[0];
        var recordSchema = RecordSchemaFactory.Create(
            rawRecordSchema,
            recordSchemaCatalog,
            new Dictionary<string, int>());

        var parameter = recordSchema.RecordPropertySchemata[0];
        if (parameter is EnumPropertySchema enumParameter)
        {
            var enumerator = Enumerable.Repeat("A", 1).GetEnumerator();
            enumParameter.CheckCompatibility(enumerator, enumMemberCatalog, logger);
        }
        else if (parameter is NullableEnumPropertySchema nullableEnumParameter)
        {
            var enumerator = Enumerable.Repeat(string.Empty, 1).GetEnumerator();
            nullableEnumParameter.CheckCompatibility(enumerator, enumMemberCatalog, logger);
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
                        [SingleColumnCatalog(",")]
                        List<{{type}}> Parameter
                     );
                     """;

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaSet = new RecordSchemaSet(loadResult, logger);
        var enumMemberCatalog = new EnumMemberCatalog(loadResult);
        var recordSchemaCatalog = new RecordSchemaCatalog(recordSchemaSet);
        RecordComplianceChecker.Check(recordSchemaCatalog, logger);

        var rawRecordSchema = recordSchemaCatalog.StaticDataRecordSchemata[0];
        var recordSchema = RecordSchemaFactory.Create(
            rawRecordSchema,
            recordSchemaCatalog,
            new Dictionary<string, int>());

        var count = Random.Shared.Next(2, 10);
        var list = Enumerable.Repeat(
            RandomValueGenerator.Generate(TypeConverter.GetSystemTypeName(type))?.ToString() ?? string.Empty,
            count);
        var argument = string.Join(", ", list);
        var arguments = Enumerable.Repeat(argument, 1).GetEnumerator();

        var parameter = recordSchema.RecordPropertySchemata[0];
        parameter.CheckCompatibility(arguments, enumMemberCatalog, logger);

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
    public void SingleColumnEnumListTest(string catalog, string type)
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
                        [SingleColumnCatalog(",")]
                        {{catalog}}<{{type}}> Parameter
                     );
                     """;

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaSet = new RecordSchemaSet(loadResult, logger);
        var enumMemberCatalog = new EnumMemberCatalog(loadResult);
        var recordSchemaCatalog = new RecordSchemaCatalog(recordSchemaSet);
        RecordComplianceChecker.Check(recordSchemaCatalog, logger);

        var rawRecordSchema = recordSchemaCatalog.StaticDataRecordSchemata[0];
        var recordSchema = RecordSchemaFactory.Create(
            rawRecordSchema,
            recordSchemaCatalog,
            new Dictionary<string, int>());

        const string argument = "A, B, C";
        var arguments = Enumerable.Repeat(argument, 1).GetEnumerator();

        var parameter = recordSchema.RecordPropertySchemata[0];
        parameter.CheckCompatibility(arguments, enumMemberCatalog, logger);

        Assert.Empty(logger.Logs);
    }
}
