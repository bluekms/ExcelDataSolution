using Microsoft.Extensions.Logging;
using SchemaInfoScanner;
using SchemaInfoScanner.Catalogs;
using SchemaInfoScanner.Collectors;
using SchemaInfoScanner.Schemata.TypedPropertySchemata;
using UnitTest.Utility;
using Xunit.Abstractions;

namespace UnitTest.NotSupportedPropertyCompatibilityTests;

public class NullableEmptySpaceCompatibilityTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void PrimitiveSetDuplicationFailTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<NullableEmptySpaceCompatibilityTests>() is not TestOutputLogger<NullableEmptySpaceCompatibilityTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         [NullString("-")]
                         [Length(5)]
                         FrozenSet<int?> Property,
                     );
                     """;

        var catalogs = CreateCatalogs(code, logger);

        var cells = new[]
        {
            new CellData("A1", "1"),
            new CellData("A2", "-"),
            new CellData("A3", "42"),
            new CellData("A4", string.Empty),
            new CellData("A5", "-7")
        };

        var context = CompatibilityContext.CreateCollectAll(catalogs.EnumMemberCatalog, cells);

        foreach (var recordSchema in catalogs.RecordSchemaCatalog.StaticDataRecordSchemata)
        {
            foreach (var propertySchema in recordSchema.PropertySchemata)
            {
                var ex = Assert.Throws<InvalidOperationException>(() => propertySchema.CheckCompatibility(context));
                logger.LogError(ex.Message, ex);
            }
        }

        Assert.Single(logger.Logs);
    }

    // TODO 유닛테스트용 Catalogs를 지우고 ExcelColumnExtractor에서 추가된 것으로 교체
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
            RecordSchemaCatalog: recordSchemaCatalog,
            EnumMemberCatalog: enumMemberCatalog);
    }
}
