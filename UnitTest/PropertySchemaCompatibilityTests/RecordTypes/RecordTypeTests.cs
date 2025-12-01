using Microsoft.Extensions.Logging;
using SchemaInfoScanner;
using SchemaInfoScanner.Catalogs;
using SchemaInfoScanner.Collectors;
using SchemaInfoScanner.Schemata.TypedPropertySchemata;
using UnitTest.Utility;
using Xunit.Abstractions;

namespace UnitTest.PropertySchemaCompatibilityTests.RecordTypes;

public class RecordTypeTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void InnerRecordTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<RecordTypeTests>() is not TestOutputLogger<RecordTypeTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         Identifier Id
                     )
                     {
                        public record struct Identifier(int Value);
                     }
                     """;

        var catalogs = CreateCatalogs(code, logger);

        var data = new[] { "1" };
        var context = CompatibilityContext.CreateNoCollect(catalogs.EnumMemberCatalog, data);

        foreach (var recordSchema in catalogs.RecordSchemaCatalog.StaticDataRecordSchemata)
        {
            foreach (var propertySchema in recordSchema.PropertySchemata)
            {
                propertySchema.CheckCompatibility(context);
            }
        }

        Assert.Empty(logger.Logs);
    }

    [Fact]
    public void AnotherRecordTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<RecordTypeTests>() is not TestOutputLogger<RecordTypeTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         MyData Data
                     );

                     public record struct MyData(int Key, string Value);
                     """;

        var catalogs = CreateCatalogs(code, logger);

        var data = new[] { "1", "AAA" };
        var context = CompatibilityContext.CreateNoCollect(catalogs.EnumMemberCatalog, data);

        foreach (var recordSchema in catalogs.RecordSchemaCatalog.StaticDataRecordSchemata)
        {
            foreach (var propertySchema in recordSchema.PropertySchemata)
            {
                propertySchema.CheckCompatibility(context);
            }
        }

        Assert.Empty(logger.Logs);
    }

    [Fact]
    public void RecordArrayTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<RecordTypeTests>() is not TestOutputLogger<RecordTypeTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         [Length(3)] ImmutableArray<MyData> Data
                     );

                     public record struct MyData(int Key, string Value);
                     """;

        var catalogs = CreateCatalogs(code, logger);

        var data = new[] { "1", "AAA", "2", "BBB", "2", "BBB" };
        var context = CompatibilityContext.CreateNoCollect(catalogs.EnumMemberCatalog, data);

        foreach (var recordSchema in catalogs.RecordSchemaCatalog.StaticDataRecordSchemata)
        {
            foreach (var propertySchema in recordSchema.PropertySchemata)
            {
                propertySchema.CheckCompatibility(context);
            }
        }

        Assert.Empty(logger.Logs);
    }

    [Fact]
    public void RecordSetTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<RecordTypeTests>() is not TestOutputLogger<RecordTypeTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         [Length(3)] FrozenSet<MyData> Data
                     );

                     public record struct MyData(int Key, string Value);
                     """;

        var catalogs = CreateCatalogs(code, logger);

        var data = new[] { "1", "AAA", "2", "BBB", "3", "CCC" };
        var context = CompatibilityContext.CreateCollectAll(catalogs.EnumMemberCatalog, data);

        foreach (var recordSchema in catalogs.RecordSchemaCatalog.StaticDataRecordSchemata)
        {
            foreach (var propertySchema in recordSchema.PropertySchemata)
            {
                propertySchema.CheckCompatibility(context);
            }
        }

        Assert.Empty(logger.Logs);
    }

    [Fact]
    public void RecordMapTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<RecordTypeTests>() is not TestOutputLogger<RecordTypeTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         [Length(3)] FrozenDictionary<int, MyData> Data
                     );

                     public record struct MyData([Key] int Id, string Value);
                     """;

        var catalogs = CreateCatalogs(code, logger);

        var data = new[] { "1", "AAA", "2", "BBB", "3", "CCC" };
        var context = CompatibilityContext.CreateCollectKey(catalogs.EnumMemberCatalog, data);

        foreach (var recordSchema in catalogs.RecordSchemaCatalog.StaticDataRecordSchemata)
        {
            foreach (var propertySchema in recordSchema.PropertySchemata)
            {
                propertySchema.CheckCompatibility(context);
            }
        }

        Assert.Empty(logger.Logs);
    }

    private record Catalogs(
        RecordSchemaCatalog RecordSchemaCatalog,
        EnumMemberCatalog EnumMemberCatalog);

    private static Catalogs CreateCatalogs(string code, ILogger logger)
    {
        var loadResult = RecordSchemaLoader.OnLoad(code, logger);
        var recordSchemaSet = new RecordSchemaSet(loadResult, logger);
        var recordSchemaCatalog = new RecordSchemaCatalog(recordSchemaSet);
        var enumMemberCatalog = new EnumMemberCatalog(loadResult);
        RecordComplianceChecker.Check(recordSchemaCatalog, logger);

        return new Catalogs(
            recordSchemaCatalog,
            enumMemberCatalog);
    }
}
