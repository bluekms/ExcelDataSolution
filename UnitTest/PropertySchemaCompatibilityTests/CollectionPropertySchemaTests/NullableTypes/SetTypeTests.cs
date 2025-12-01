using Microsoft.Extensions.Logging;
using SchemaInfoScanner;
using SchemaInfoScanner.Catalogs;
using SchemaInfoScanner.Collectors;
using SchemaInfoScanner.Schemata.TypedPropertySchemata;
using UnitTest.Utility;
using Xunit.Abstractions;

namespace UnitTest.PropertySchemaCompatibilityTests.CollectionPropertySchemaTests.NullableTypes;

public class SetTypeTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void PrimitiveSetTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<SetTypeTests>() is not TestOutputLogger<SetTypeTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         [NullString("-")]
                         [Length(4)]
                         FrozenSet<int?> Property,
                     );
                     """;

        var catalogs = CreateCatalogs(code, logger);

        var data = new[] { "1", "42", "-", "-7" };
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
    public void PrimitiveSetDuplicationFailTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<SetTypeTests>() is not TestOutputLogger<SetTypeTests> logger)
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

        var data = new[] { "1", "-", "42", "-", "-7" };
        var context = CompatibilityContext.CreateCollectAll(catalogs.EnumMemberCatalog, data);

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

    [Fact]
    public void EnumSetTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<SetTypeTests>() is not TestOutputLogger<SetTypeTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     public enum MyEnum { A, B, C }

                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         [NullString("-")]
                         [Length(3)]
                         FrozenSet<MyEnum?> Property,
                     );
                     """;

        var catalogs = CreateCatalogs(code, logger);

        var data = new[] { "B", "A", "-" };
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
    public void DateTimeSetTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<SetTypeTests>() is not TestOutputLogger<SetTypeTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         [DateTimeFormat("yyyy-MM-dd HH:mm:ss.fff")]
                         [NullString("-")]
                         [Length(3)]
                         FrozenSet<DateTime?> Property,
                     );
                     """;

        var catalogs = CreateCatalogs(code, logger);

        var data = new[] { "-", "1986-05-26 01:05:00.000", "1993-12-28 01:05:00.000" };
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
    public void TimeSpanSetTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<SetTypeTests>() is not TestOutputLogger<SetTypeTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         [TimeSpanFormat("c")]
                         [NullString("-")]
                         [Length(2)]
                         FrozenSet<TimeSpan?> Property,
                     );
                     """;

        var catalogs = CreateCatalogs(code, logger);

        var data = new[] { "1.02:03:04.5670000", "2.02:03:04.5670000" };
        var context = CompatibilityContext.CreateCollectAll(catalogs.EnumMemberCatalog, data, 0);

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
    public void SingleColumnSetTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<SetTypeTests>() is not TestOutputLogger<SetTypeTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         [NullString("")]
                         [SingleColumnCollection(", ")]
                         FrozenSet<int?> Property,
                     );
                     """;

        var catalogs = CreateCatalogs(code, logger);
        var context = CompatibilityContext.CreateCollectAll(catalogs.EnumMemberCatalog, ["1, 42, , -7"]);

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
    public void SingleColumnEnumSetTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<SetTypeTests>() is not TestOutputLogger<SetTypeTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     public enum MyEnum { A, B, C }

                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         [SingleColumnCollection(", ")]
                         [NullString("")]
                         FrozenSet<MyEnum?> Property,
                     );
                     """;

        var catalogs = CreateCatalogs(code, logger);
        var context = CompatibilityContext.CreateCollectAll(catalogs.EnumMemberCatalog, ["C, A, "]);

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
    public void SingleColumnDateTimeSetTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<SetTypeTests>() is not TestOutputLogger<SetTypeTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         [SingleColumnCollection(", ")]
                         [NullString("")]
                         [DateTimeFormat("yyyy-MM-dd HH:mm:ss.fff")]
                         FrozenSet<DateTime?> Property,
                     );
                     """;

        var catalogs = CreateCatalogs(code, logger);
        var context = CompatibilityContext.CreateCollectAll(catalogs.EnumMemberCatalog, [", 1986-05-26 01:05:00.000, 1993-12-28 01:05:00.000"]);

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
    public void SingleColumnTimeSpanSetTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<SetTypeTests>() is not TestOutputLogger<SetTypeTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         [SingleColumnCollection(", ")]
                         [TimeSpanFormat("c")]
                         [NullString("")]
                         FrozenSet<TimeSpan?> Property,
                     );
                     """;

        var catalogs = CreateCatalogs(code, logger);
        var context = CompatibilityContext.CreateCollectAll(catalogs.EnumMemberCatalog, ["1.02:03:04.5670000, , 2.02:03:04.5670000"]);

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
