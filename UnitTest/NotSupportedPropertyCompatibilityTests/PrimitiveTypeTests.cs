using Microsoft.Extensions.Logging;
using SchemaInfoScanner;
using SchemaInfoScanner.Catalogs;
using SchemaInfoScanner.Collectors;
using SchemaInfoScanner.Schemata.TypedPropertySchemata;
using UnitTest.Utility;
using Xunit.Abstractions;

namespace UnitTest.NotSupportedPropertyCompatibilityTests;

public class PrimitiveTypeTests(ITestOutputHelper testOutputHelper)
{
    [Theory]
    [InlineData("bool", "0")]
    [InlineData("bool", "1")]
    [InlineData("bool", "참")]
    [InlineData("bool", "거짓")]
    public void BooleanTest(string type, string argument)
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<PrimitiveTypeTests>() is not TestOutputLogger<PrimitiveTypeTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         {{type}} Property,
                     );
                     """;

        var catalogs = CreateCatalogs(code, logger);
        foreach (var recordSchema in catalogs.RecordSchemaCatalog.StaticDataRecordSchemata)
        {
            foreach (var propertySchema in recordSchema.RecordPropertySchemata)
            {
                var context = new CompatibilityContext(
                    catalogs.EnumMemberCatalog,
                    Enumerable.Repeat(argument, 1).ToList());

                Assert.Throws<FormatException>(() => propertySchema.CheckCompatibility(context));
            }
        }

        Assert.Single(logger.Logs);
    }

    [Theory]
    [InlineData("byte", "ff")]
    [InlineData("byte", "0xff")]
    [InlineData("byte", "FF")]
    [InlineData("byte", "0xFF")]
    public void ByteTest(string type, string argument)
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<PrimitiveTypeTests>() is not TestOutputLogger<PrimitiveTypeTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         {{type}} Property,
                     );
                     """;

        var catalogs = CreateCatalogs(code, logger);
        foreach (var recordSchema in catalogs.RecordSchemaCatalog.StaticDataRecordSchemata)
        {
            foreach (var propertySchema in recordSchema.RecordPropertySchemata)
            {
                var context = new CompatibilityContext(
                    catalogs.EnumMemberCatalog,
                    Enumerable.Repeat(argument, 1).ToList());

                Assert.Throws<FormatException>(() => propertySchema.CheckCompatibility(context));
            }
        }

        Assert.Single(logger.Logs);
    }

    private record Catalogs(
        RecordSchemaCatalog RecordSchemaCatalog,
        EnumMemberCatalog EnumMemberCatalog);

    private static Catalogs CreateCatalogs(string code, ILogger logger)
    {
        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaSet = new RecordSchemaSet(loadResult, logger);
        var recordSchemaCatalog = new RecordSchemaCatalog(recordSchemaSet);
        var enumMemberCatalog = new EnumMemberCatalog(loadResult);
        RecordComplianceChecker.Check(recordSchemaCatalog, logger);

        return new Catalogs(
            RecordSchemaCatalog: recordSchemaCatalog,
            EnumMemberCatalog: enumMemberCatalog);
    }
}
