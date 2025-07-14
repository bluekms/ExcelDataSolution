using Microsoft.Extensions.Logging;
using SchemaInfoScanner;
using SchemaInfoScanner.Catalogs;
using SchemaInfoScanner.Collectors;
using SchemaInfoScanner.Schemata.TypedPropertySchemata;
using UnitTest.Utility;
using Xunit.Abstractions;

namespace UnitTest.PropertySchemaCompatibilityTests.CollectionPropertySchemaTests;

public class HashSetTypeTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void PrimitiveHashSetTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<HashSetTypeTests>() is not TestOutputLogger<HashSetTypeTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         FrozenSet<int> Property,
                     );
                     """;

        var catalogs = CreateCatalogs(code, logger);

        var data = new[] { "1", "42", "0", "-7" };
        var context = new CompatibilityContext(catalogs.EnumMemberCatalog, data, 0, data.Length);

        foreach (var recordSchema in catalogs.RecordSchemaCatalog.StaticDataRecordSchemata)
        {
            foreach (var propertySchema in recordSchema.RecordPropertySchemata)
            {
                propertySchema.CheckCompatibility(context);
            }
        }

        Assert.Empty(logger.Logs);
    }

    [Fact]
    public void PrimitiveHashSetDuplicationFailTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<HashSetTypeTests>() is not TestOutputLogger<HashSetTypeTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         FrozenSet<int> Property,
                     );
                     """;

        var catalogs = CreateCatalogs(code, logger);

        var data = new[] { "1", "0", "-7", "-7" };
        var context = new CompatibilityContext(catalogs.EnumMemberCatalog, data, 0, data.Length);

        foreach (var recordSchema in catalogs.RecordSchemaCatalog.StaticDataRecordSchemata)
        {
            foreach (var propertySchema in recordSchema.RecordPropertySchemata)
            {
                var ex = Assert.Throws<InvalidOperationException>(() => propertySchema.CheckCompatibility(context));
                logger.LogError(ex.Message, ex);
            }
        }

        Assert.Single(logger.Logs);
    }

    [Fact]
    public void EnumHashSetTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<HashSetTypeTests>() is not TestOutputLogger<HashSetTypeTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     public enum MyEnum { A, a, C }

                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         FrozenSet<MyEnum> Property,
                     );
                     """;

        var catalogs = CreateCatalogs(code, logger);

        var data = new[] { "a", "A" };
        var context = new CompatibilityContext(catalogs.EnumMemberCatalog, data, 0, data.Length);

        foreach (var recordSchema in catalogs.RecordSchemaCatalog.StaticDataRecordSchemata)
        {
            foreach (var propertySchema in recordSchema.RecordPropertySchemata)
            {
                propertySchema.CheckCompatibility(context);
            }
        }

        Assert.Empty(logger.Logs);
    }

    [Fact]
    public void EnumHashSetDuplicationFailTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<HashSetTypeTests>() is not TestOutputLogger<HashSetTypeTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     public enum MyEnum { A, B, C }

                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         FrozenSet<MyEnum> Property,
                     );
                     """;

        var catalogs = CreateCatalogs(code, logger);

        var data = new[] { "C", "A", "A" };
        var context = new CompatibilityContext(catalogs.EnumMemberCatalog, data, 0, data.Length);

        foreach (var recordSchema in catalogs.RecordSchemaCatalog.StaticDataRecordSchemata)
        {
            foreach (var propertySchema in recordSchema.RecordPropertySchemata)
            {
                var ex = Assert.Throws<InvalidOperationException>(() => propertySchema.CheckCompatibility(context));
                logger.LogError(ex.Message, ex);
            }
        }

        Assert.Single(logger.Logs);
    }

    [Fact]
    public void DateTimeHashSetTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<HashSetTypeTests>() is not TestOutputLogger<HashSetTypeTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         [DateTimeFormat("yyyy-MM-dd HH:mm:ss.fff")]
                         FrozenSet<DateTime> Property,
                     );
                     """;

        var catalogs = CreateCatalogs(code, logger);

        var data = new[] { "1986-05-26 01:05:00.000", "1993-12-28 01:05:00.000" };
        var context = new CompatibilityContext(catalogs.EnumMemberCatalog, data, 0, data.Length);

        foreach (var recordSchema in catalogs.RecordSchemaCatalog.StaticDataRecordSchemata)
        {
            foreach (var propertySchema in recordSchema.RecordPropertySchemata)
            {
                propertySchema.CheckCompatibility(context);
            }
        }

        Assert.Empty(logger.Logs);
    }

    [Fact]
    public void TimeSpanHashSetTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<HashSetTypeTests>() is not TestOutputLogger<HashSetTypeTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         [TimeSpanFormat("c")]
                         FrozenSet<TimeSpan> Property,
                     );
                     """;

        var catalogs = CreateCatalogs(code, logger);

        var data = new[] { "1.02:03:04.5670000", "2.02:03:04.5670000" };
        var context = new CompatibilityContext(catalogs.EnumMemberCatalog, data, 0, data.Length);

        foreach (var recordSchema in catalogs.RecordSchemaCatalog.StaticDataRecordSchemata)
        {
            foreach (var propertySchema in recordSchema.RecordPropertySchemata)
            {
                propertySchema.CheckCompatibility(context);
            }
        }

        Assert.Empty(logger.Logs);
    }

    [Fact]
    public void SingleColumnHashSetTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<HashSetTypeTests>() is not TestOutputLogger<HashSetTypeTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         [SingleColumnCollection(", ")]
                         FrozenSet<int> Property,
                     );
                     """;

        var catalogs = CreateCatalogs(code, logger);
        var context = new CompatibilityContext(catalogs.EnumMemberCatalog, ["1, 42, 0, -7"]);

        foreach (var recordSchema in catalogs.RecordSchemaCatalog.StaticDataRecordSchemata)
        {
            foreach (var propertySchema in recordSchema.RecordPropertySchemata)
            {
                propertySchema.CheckCompatibility(context);
            }
        }

        Assert.Empty(logger.Logs);
    }

    [Fact]
    public void SingleColumnHashSetDuplicationFailTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<HashSetTypeTests>() is not TestOutputLogger<HashSetTypeTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         [SingleColumnCollection(", ")]
                         FrozenSet<int> Property,
                     );
                     """;

        var catalogs = CreateCatalogs(code, logger);
        var context = new CompatibilityContext(catalogs.EnumMemberCatalog, ["-7, 42, 0, -7"]);

        foreach (var recordSchema in catalogs.RecordSchemaCatalog.StaticDataRecordSchemata)
        {
            foreach (var propertySchema in recordSchema.RecordPropertySchemata)
            {
                var ex = Assert.Throws<InvalidOperationException>(() => propertySchema.CheckCompatibility(context));
                logger.LogError(ex.Message, ex);
            }
        }

        Assert.Single(logger.Logs);
    }

    [Fact]
    public void NullablePrimitiveHashSetTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<HashSetTypeTests>() is not TestOutputLogger<HashSetTypeTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         [NullString("-")]
                         FrozenSet<int?> Property,
                     );
                     """;

        var catalogs = CreateCatalogs(code, logger);

        var data = new[] { "1", "42", "-", "-7" };
        var context = new CompatibilityContext(catalogs.EnumMemberCatalog, data, 0, data.Length);

        foreach (var recordSchema in catalogs.RecordSchemaCatalog.StaticDataRecordSchemata)
        {
            foreach (var propertySchema in recordSchema.RecordPropertySchemata)
            {
                propertySchema.CheckCompatibility(context);
            }
        }

        Assert.Empty(logger.Logs);
    }

    [Fact]
    public void NullablePrimitiveHashSetDuplicationFailTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<HashSetTypeTests>() is not TestOutputLogger<HashSetTypeTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         [NullString("-")]
                         FrozenSet<int?> Property,
                     );
                     """;

        var catalogs = CreateCatalogs(code, logger);

        var data = new[] { "1", "-", "42", "-", "-7" };
        var context = new CompatibilityContext(catalogs.EnumMemberCatalog, data, 0, data.Length);

        foreach (var recordSchema in catalogs.RecordSchemaCatalog.StaticDataRecordSchemata)
        {
            foreach (var propertySchema in recordSchema.RecordPropertySchemata)
            {
                var ex = Assert.Throws<InvalidOperationException>(() => propertySchema.CheckCompatibility(context));
                logger.LogError(ex.Message, ex);
            }
        }

        Assert.Single(logger.Logs);
    }

    [Fact]
    public void NullableEnumHashSetTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<HashSetTypeTests>() is not TestOutputLogger<HashSetTypeTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     public enum MyEnum { A, B, C }

                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         [NullString("-")]
                         FrozenSet<MyEnum?> Property,
                     );
                     """;

        var catalogs = CreateCatalogs(code, logger);

        var data = new[] { "B", "A", "-" };
        var context = new CompatibilityContext(catalogs.EnumMemberCatalog, data, 0, data.Length);

        foreach (var recordSchema in catalogs.RecordSchemaCatalog.StaticDataRecordSchemata)
        {
            foreach (var propertySchema in recordSchema.RecordPropertySchemata)
            {
                propertySchema.CheckCompatibility(context);
            }
        }

        Assert.Empty(logger.Logs);
    }

    [Fact]
    public void NullableDateTimeHashSetTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<HashSetTypeTests>() is not TestOutputLogger<HashSetTypeTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         [DateTimeFormat("yyyy-MM-dd HH:mm:ss.fff")]
                         [NullString("-")]
                         FrozenSet<DateTime?> Property,
                     );
                     """;

        var catalogs = CreateCatalogs(code, logger);

        var data = new[] { "-", "1986-05-26 01:05:00.000", "1993-12-28 01:05:00.000" };
        var context = new CompatibilityContext(catalogs.EnumMemberCatalog, data, 0, data.Length);

        foreach (var recordSchema in catalogs.RecordSchemaCatalog.StaticDataRecordSchemata)
        {
            foreach (var propertySchema in recordSchema.RecordPropertySchemata)
            {
                propertySchema.CheckCompatibility(context);
            }
        }

        Assert.Empty(logger.Logs);
    }

    [Fact]
    public void NullableTimeSpanHashSetTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<HashSetTypeTests>() is not TestOutputLogger<HashSetTypeTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         [TimeSpanFormat("c")]
                         [NullString("-")]
                         FrozenSet<TimeSpan?> Property,
                     );
                     """;

        var catalogs = CreateCatalogs(code, logger);

        var data = new[] { "1.02:03:04.5670000", "2.02:03:04.5670000" };
        var context = new CompatibilityContext(catalogs.EnumMemberCatalog, data, 0, data.Length);

        foreach (var recordSchema in catalogs.RecordSchemaCatalog.StaticDataRecordSchemata)
        {
            foreach (var propertySchema in recordSchema.RecordPropertySchemata)
            {
                propertySchema.CheckCompatibility(context);
            }
        }

        Assert.Empty(logger.Logs);
    }

    [Fact]
    public void NullableSingleColumnHashSetTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<HashSetTypeTests>() is not TestOutputLogger<HashSetTypeTests> logger)
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
        var context = new CompatibilityContext(catalogs.EnumMemberCatalog, ["1, 42, , -7"]);

        foreach (var recordSchema in catalogs.RecordSchemaCatalog.StaticDataRecordSchemata)
        {
            foreach (var propertySchema in recordSchema.RecordPropertySchemata)
            {
                propertySchema.CheckCompatibility(context);
            }
        }

        Assert.Empty(logger.Logs);
    }

    [Fact]
    public void NullableSingleColumnEnumHashSetTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<HashSetTypeTests>() is not TestOutputLogger<HashSetTypeTests> logger)
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
        var context = new CompatibilityContext(catalogs.EnumMemberCatalog, ["C, A, "]);

        foreach (var recordSchema in catalogs.RecordSchemaCatalog.StaticDataRecordSchemata)
        {
            foreach (var propertySchema in recordSchema.RecordPropertySchemata)
            {
                propertySchema.CheckCompatibility(context);
            }
        }

        Assert.Empty(logger.Logs);
    }

    [Fact]
    public void NullableSingleColumnDateTimeHashSetTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<HashSetTypeTests>() is not TestOutputLogger<HashSetTypeTests> logger)
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
        var context = new CompatibilityContext(catalogs.EnumMemberCatalog, [", 1986-05-26 01:05:00.000, 1993-12-28 01:05:00.000"]);

        foreach (var recordSchema in catalogs.RecordSchemaCatalog.StaticDataRecordSchemata)
        {
            foreach (var propertySchema in recordSchema.RecordPropertySchemata)
            {
                propertySchema.CheckCompatibility(context);
            }
        }

        Assert.Empty(logger.Logs);
    }

    [Fact]
    public void NullableSingleColumnTimeSpanHashSetTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<HashSetTypeTests>() is not TestOutputLogger<HashSetTypeTests> logger)
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
        var context = new CompatibilityContext(catalogs.EnumMemberCatalog, ["1.02:03:04.5670000, , 2.02:03:04.5670000"]);

        foreach (var recordSchema in catalogs.RecordSchemaCatalog.StaticDataRecordSchemata)
        {
            foreach (var propertySchema in recordSchema.RecordPropertySchemata)
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
