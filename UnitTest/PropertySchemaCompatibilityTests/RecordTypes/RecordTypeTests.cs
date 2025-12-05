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

        var data = new[]
        {
            new CellData("A1", "1")
        };

        var context = CompatibilityContext.CreateNoCollect(catalogs, data);

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

        var data = new[]
        {
            new CellData("A1", "1"),
            new CellData("A2", "AAA")
        };

        var context = CompatibilityContext.CreateNoCollect(catalogs, data);

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

        var data = new[]
        {
            new CellData("A1", "1"),
            new CellData("A2", "AAA"),
            new CellData("A3", "2"),
            new CellData("A4", "BBB"),
            new CellData("A5", "2"),
            new CellData("A6", "BBB")
        };

        var context = CompatibilityContext.CreateNoCollect(catalogs, data);

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

        var data = new[]
        {
            new CellData("A1", "1"),
            new CellData("A2", "AAA"),
            new CellData("A3", "2"),
            new CellData("A4", "BBB"),
            new CellData("A5", "3"),
            new CellData("A6", "CCC")
        };

        var context = CompatibilityContext.CreateCollectAll(catalogs, data);

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

        var data = new[]
        {
            new CellData("A1", "1"),
            new CellData("A2", "AAA"),
            new CellData("A3", "2"),
            new CellData("A4", "BBB"),
            new CellData("A5", "3"),
            new CellData("A6", "CCC")
        };

        var context = CompatibilityContext.CreateCollectKey(catalogs, data);

        foreach (var recordSchema in catalogs.RecordSchemaCatalog.StaticDataRecordSchemata)
        {
            foreach (var propertySchema in recordSchema.PropertySchemata)
            {
                propertySchema.CheckCompatibility(context);
            }
        }

        Assert.Empty(logger.Logs);
    }

    private static MetadataCatalogs CreateCatalogs(string code, ILogger logger)
    {
        var loadResult = RecordSchemaLoader.OnLoad(code, logger);
        var recordSchemaSet = new RecordSchemaSet(loadResult, logger);
        var recordSchemaCatalog = new RecordSchemaCatalog(recordSchemaSet);
        var enumMemberCatalog = new EnumMemberCatalog(loadResult);
        RecordComplianceChecker.Check(recordSchemaCatalog, logger);

        return new(recordSchemaCatalog, enumMemberCatalog);
    }
}
