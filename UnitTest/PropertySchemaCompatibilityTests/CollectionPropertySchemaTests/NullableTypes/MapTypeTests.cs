using Microsoft.Extensions.Logging;
using SchemaInfoScanner;
using SchemaInfoScanner.Catalogs;
using SchemaInfoScanner.Collectors;
using SchemaInfoScanner.Schemata.TypedPropertySchemata;
using UnitTest.Utility;
using Xunit.Abstractions;

namespace UnitTest.PropertySchemaCompatibilityTests.CollectionPropertySchemaTests.NullableTypes;

public class MapTypeTests(ITestOutputHelper testOutputHelper)
{
    [Theory]
    [InlineData("sbyte", new[] { "0", "1", "2", "3" }, new[] { "Hello", "", "World", "!" })]
    public void PrimitiveKeyNullablePrimitiveValueMapTest(string keyType, string[] keys, string[] values)
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<MapTypeTests>() is not TestOutputLogger<MapTypeTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         [NullString("")]
                         [Length(3)]
                         FrozenDictionary<{{keyType}}, string?> Property,
                     );
                     """;

        var catalogs = CreateCatalogs(code, logger);

        var data = MakeDictionaryRawData(keys, values);
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

    private static MetadataCatalogs CreateCatalogs(string code, ILogger logger)
    {
        var loadResult = RecordSchemaLoader.OnLoad(code, logger);
        var recordSchemaSet = new RecordSchemaSet(loadResult, logger);
        var recordSchemaCatalog = new RecordSchemaCatalog(recordSchemaSet);
        var enumMemberCatalog = new EnumMemberCatalog(loadResult);
        RecordComplianceChecker.Check(recordSchemaCatalog, logger);

        return new(recordSchemaCatalog, enumMemberCatalog);
    }

    private static CellData[] MakeDictionaryRawData(string[] keys, string[] values)
    {
        if (keys.Length != values.Length)
        {
            throw new ArgumentException("Keys and values must have the same length.");
        }

        var cells = new List<CellData>();
        var row = 1;

        for (var i = 0; i < keys.Length; i++)
        {
            cells.Add(new CellData($"A{row++}", keys[i]));
            cells.Add(new CellData($"A{row++}", values[i]));
        }

        return cells.ToArray();
    }
}
