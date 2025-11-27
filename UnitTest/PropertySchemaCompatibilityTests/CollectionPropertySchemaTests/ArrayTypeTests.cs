using Microsoft.Extensions.Logging;
using SchemaInfoScanner;
using SchemaInfoScanner.Catalogs;
using SchemaInfoScanner.Collectors;
using SchemaInfoScanner.Schemata.TypedPropertySchemata;
using UnitTest.Utility;
using Xunit.Abstractions;

namespace UnitTest.PropertySchemaCompatibilityTests.CollectionPropertySchemaTests;

public class ArrayTypeTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void PrimitiveArrayTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<ArrayTypeTests>() is not TestOutputLogger<ArrayTypeTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         [Length(3)]
                         ImmutableArray<int> Property,
                     );
                     """;

        var catalogs = CreateCatalogs(code, logger);

        var data = new[] { "1", "42", "0", "-7" };
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
    public void EnumArrayTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<ArrayTypeTests>() is not TestOutputLogger<ArrayTypeTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     public enum MyEnum { A, B, C }

                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         [Length(2)]
                         ImmutableArray<MyEnum> Property,
                     );
                     """;

        var catalogs = CreateCatalogs(code, logger);

        var data = new[] { "C", "A" };
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
    public void DateTimeArrayTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<ArrayTypeTests>() is not TestOutputLogger<ArrayTypeTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         [DateTimeFormat("yyyy-MM-dd HH:mm:ss.fff")]
                         [Length(2)]
                         ImmutableArray<DateTime> Property,
                     );
                     """;

        var catalogs = CreateCatalogs(code, logger);

        var data = new[] { "1986-05-26 01:05:00.000", "1993-12-28 01:05:00.000" };
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
    public void TimeSpanArrayTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<ArrayTypeTests>() is not TestOutputLogger<ArrayTypeTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         [TimeSpanFormat("c")]
                         [Length(2)]
                         ImmutableArray<TimeSpan> Property,
                     );
                     """;

        var catalogs = CreateCatalogs(code, logger);

        var data = new[] { "1.02:03:04.5670000", "2.02:03:04.5670000" };
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
    public void SingleColumnArrayTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<ArrayTypeTests>() is not TestOutputLogger<ArrayTypeTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         [SingleColumnCollection(", ")]
                         [Length(4)]
                         ImmutableArray<int> Property,
                     );
                     """;

        var catalogs = CreateCatalogs(code, logger);
        var context = CompatibilityContext.CreateNoCollect(catalogs.EnumMemberCatalog, ["1, 42, 0, -7"]);

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
    public void NullablePrimitiveArrayTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<ArrayTypeTests>() is not TestOutputLogger<ArrayTypeTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         [Length(3)][NullString("-")] ImmutableArray<int?> Property,
                     );
                     """;

        var catalogs = CreateCatalogs(code, logger);

        var data = new[] { "1", "42", "-", "-7" };
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
    public void NullableEnumArrayTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<ArrayTypeTests>() is not TestOutputLogger<ArrayTypeTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     public enum MyEnum { A, B, C }

                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         [Length(3)][NullString("-")] ImmutableArray<MyEnum?> Property,
                     );
                     """;

        var catalogs = CreateCatalogs(code, logger);

        var data = new[] { "B", "A", "-" };
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
    public void NullableDateTimeArrayTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<ArrayTypeTests>() is not TestOutputLogger<ArrayTypeTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         [DateTimeFormat("yyyy-MM-dd HH:mm:ss.fff")]
                         [NullString("-")]
                         [Length(3)]
                         ImmutableArray<DateTime?> Property,
                     );
                     """;

        var catalogs = CreateCatalogs(code, logger);

        var data = new[] { "-", "1986-05-26 01:05:00.000", "1993-12-28 01:05:00.000" };
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
    public void NullableTimeSpanArrayTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<ArrayTypeTests>() is not TestOutputLogger<ArrayTypeTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         [TimeSpanFormat("c")]
                         [NullString("-")]
                         [Length(2)]
                         ImmutableArray<TimeSpan?> Property,
                     );
                     """;

        var catalogs = CreateCatalogs(code, logger);

        var data = new[] { "1.02:03:04.5670000", "2.02:03:04.5670000" };
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
    public void NullableSingleColumnArrayTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<ArrayTypeTests>() is not TestOutputLogger<ArrayTypeTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         [NullString("")]
                         [SingleColumnCollection(", ")]
                         ImmutableArray<int?> Property,
                     );
                     """;

        var catalogs = CreateCatalogs(code, logger);
        var context = CompatibilityContext.CreateNoCollect(catalogs.EnumMemberCatalog, ["1, 42, , -7"]);

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
    public void NullableSingleColumnEnumArrayTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<ArrayTypeTests>() is not TestOutputLogger<ArrayTypeTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     public enum MyEnum { A, B, C }

                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         [SingleColumnCollection(", ")]
                         [NullString("")]
                         ImmutableArray<MyEnum?> Property,
                     );
                     """;

        var catalogs = CreateCatalogs(code, logger);
        var context = CompatibilityContext.CreateNoCollect(catalogs.EnumMemberCatalog, ["C, A, "]);

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
    public void NullableSingleColumnDateTimeArrayTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<ArrayTypeTests>() is not TestOutputLogger<ArrayTypeTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         [SingleColumnCollection(", ")]
                         [NullString("")]
                         [DateTimeFormat("yyyy-MM-dd HH:mm:ss.fff")]
                         ImmutableArray<DateTime?> Property,
                     );
                     """;

        var catalogs = CreateCatalogs(code, logger);
        var context = CompatibilityContext.CreateNoCollect(catalogs.EnumMemberCatalog, [", 1986-05-26 01:05:00.000, 1993-12-28 01:05:00.000"]);

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
    public void NullableSingleColumnTimeSpanArrayTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<ArrayTypeTests>() is not TestOutputLogger<ArrayTypeTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         [SingleColumnCollection(", ")]
                         [TimeSpanFormat("c")]
                         [NullString("")]
                         ImmutableArray<TimeSpan?> Property,
                     );
                     """;

        var catalogs = CreateCatalogs(code, logger);
        var context = CompatibilityContext.CreateNoCollect(catalogs.EnumMemberCatalog, ["1.02:03:04.5670000, , 2.02:03:04.5670000"]);

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
