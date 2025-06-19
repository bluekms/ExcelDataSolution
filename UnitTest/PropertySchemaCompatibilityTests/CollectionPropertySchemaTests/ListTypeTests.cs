using Microsoft.Extensions.Logging;
using SchemaInfoScanner;
using SchemaInfoScanner.Catalogs;
using SchemaInfoScanner.Collectors;
using SchemaInfoScanner.Schemata.CompatibilityContexts;
using UnitTest.Utility;
using Xunit.Abstractions;

namespace UnitTest.PropertySchemaCompatibilityTests.CollectionPropertySchemaTests;

public class ListTypeTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void PrimitiveListTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<ListTypeTests>() is not TestOutputLogger<ListTypeTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         IReadOnlyList<int> Property,
                     );
                     """;

        var catalogs = CreateCatalogs(code, logger);

        var data = new[] { "1", "42", "0", "-7" };
        var context = new CompatibilityContext(catalogs.EnumMemberCatalog, data, 0, data.Length);

        foreach (var recordSchema in catalogs.RecordSchemaCatalog.StaticDataRecordSchemata)
        {
            foreach (var propertySchema in recordSchema.RecordPropertySchemata)
            {
                propertySchema.CheckCompatibility(context, logger);
            }
        }

        Assert.Empty(logger.Logs);
    }

    [Fact]
    public void EnumListTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<ListTypeTests>() is not TestOutputLogger<ListTypeTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     public enum MyEnum { A, B, C }

                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         IReadOnlyList<MyEnum> Property,
                     );
                     """;

        var catalogs = CreateCatalogs(code, logger);

        var data = new[] { "C", "A" };
        var context = new CompatibilityContext(catalogs.EnumMemberCatalog, data, 0, data.Length);

        foreach (var recordSchema in catalogs.RecordSchemaCatalog.StaticDataRecordSchemata)
        {
            foreach (var propertySchema in recordSchema.RecordPropertySchemata)
            {
                propertySchema.CheckCompatibility(context, logger);
            }
        }

        Assert.Empty(logger.Logs);
    }

    [Fact]
    public void DateTimeListTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<ListTypeTests>() is not TestOutputLogger<ListTypeTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         [DateTimeFormat("yyyy-MM-dd HH:mm:ss.fff")]
                         IReadOnlyList<DateTime> Property,
                     );
                     """;

        var catalogs = CreateCatalogs(code, logger);

        var data = new[] { "1986-05-26 01:05:00.000", "1993-12-28 01:05:00.000" };
        var context = new CompatibilityContext(catalogs.EnumMemberCatalog, data, 0, data.Length);

        foreach (var recordSchema in catalogs.RecordSchemaCatalog.StaticDataRecordSchemata)
        {
            foreach (var propertySchema in recordSchema.RecordPropertySchemata)
            {
                propertySchema.CheckCompatibility(context, logger);
            }
        }

        Assert.Empty(logger.Logs);
    }

    [Fact]
    public void TimeSpanListTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<ListTypeTests>() is not TestOutputLogger<ListTypeTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         [TimeSpanFormat("c")]
                         IReadOnlyList<TimeSpan> Property,
                     );
                     """;

        var catalogs = CreateCatalogs(code, logger);

        var data = new[] { "1.02:03:04.5670000", "2.02:03:04.5670000" };
        var context = new CompatibilityContext(catalogs.EnumMemberCatalog, data, 0, data.Length);

        foreach (var recordSchema in catalogs.RecordSchemaCatalog.StaticDataRecordSchemata)
        {
            foreach (var propertySchema in recordSchema.RecordPropertySchemata)
            {
                propertySchema.CheckCompatibility(context, logger);
            }
        }

        Assert.Empty(logger.Logs);
    }

    [Fact]
    public void SingleColumnListTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<ListTypeTests>() is not TestOutputLogger<ListTypeTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         [SingleColumnCollection(", ")] IReadOnlyList<int> Property,
                     );
                     """;

        var catalogs = CreateCatalogs(code, logger);
        var context = new CompatibilityContext(catalogs.EnumMemberCatalog, ["1, 42, 0, -7"]);

        foreach (var recordSchema in catalogs.RecordSchemaCatalog.StaticDataRecordSchemata)
        {
            foreach (var propertySchema in recordSchema.RecordPropertySchemata)
            {
                propertySchema.CheckCompatibility(context, logger);
            }
        }

        Assert.Empty(logger.Logs);
    }

    [Fact]
    public void NullablePrimitiveListTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<ListTypeTests>() is not TestOutputLogger<ListTypeTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         [NullString("-")] IReadOnlyList<int?> Property,
                     );
                     """;

        var catalogs = CreateCatalogs(code, logger);

        var data = new[] { "1", "42", "-", "-7" };
        var context = new CompatibilityContext(catalogs.EnumMemberCatalog, data, 0, data.Length);

        foreach (var recordSchema in catalogs.RecordSchemaCatalog.StaticDataRecordSchemata)
        {
            foreach (var propertySchema in recordSchema.RecordPropertySchemata)
            {
                propertySchema.CheckCompatibility(context, logger);
            }
        }

        Assert.Empty(logger.Logs);
    }

    [Fact]
    public void NullableEnumListTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<ListTypeTests>() is not TestOutputLogger<ListTypeTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     public enum MyEnum { A, B, C }

                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         [NullString("-")] IReadOnlyList<MyEnum?> Property,
                     );
                     """;

        var catalogs = CreateCatalogs(code, logger);

        var data = new[] { "B", "A", "-" };
        var context = new CompatibilityContext(catalogs.EnumMemberCatalog, data, 0, data.Length);

        foreach (var recordSchema in catalogs.RecordSchemaCatalog.StaticDataRecordSchemata)
        {
            foreach (var propertySchema in recordSchema.RecordPropertySchemata)
            {
                propertySchema.CheckCompatibility(context, logger);
            }
        }

        Assert.Empty(logger.Logs);
    }

    [Fact]
    public void NullableDateTimeListTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<ListTypeTests>() is not TestOutputLogger<ListTypeTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         [DateTimeFormat("yyyy-MM-dd HH:mm:ss.fff")]
                         [NullString("-")]
                         IReadOnlyList<DateTime?> Property,
                     );
                     """;

        var catalogs = CreateCatalogs(code, logger);

        var data = new[] { "-", "1986-05-26 01:05:00.000", "1993-12-28 01:05:00.000" };
        var context = new CompatibilityContext(catalogs.EnumMemberCatalog, data, 0, data.Length);

        foreach (var recordSchema in catalogs.RecordSchemaCatalog.StaticDataRecordSchemata)
        {
            foreach (var propertySchema in recordSchema.RecordPropertySchemata)
            {
                propertySchema.CheckCompatibility(context, logger);
            }
        }

        Assert.Empty(logger.Logs);
    }

    [Fact]
    public void NullableTimeSpanListTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<ListTypeTests>() is not TestOutputLogger<ListTypeTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         [TimeSpanFormat("c")]
                         [NullString("-")]
                         IReadOnlyList<TimeSpan?> Property,
                     );
                     """;

        var catalogs = CreateCatalogs(code, logger);

        var data = new[] { "1.02:03:04.5670000", "2.02:03:04.5670000" };
        var context = new CompatibilityContext(catalogs.EnumMemberCatalog, data, 0, data.Length);

        foreach (var recordSchema in catalogs.RecordSchemaCatalog.StaticDataRecordSchemata)
        {
            foreach (var propertySchema in recordSchema.RecordPropertySchemata)
            {
                propertySchema.CheckCompatibility(context, logger);
            }
        }

        Assert.Empty(logger.Logs);
    }

    [Fact]
    public void NullableSingleColumnListTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<ListTypeTests>() is not TestOutputLogger<ListTypeTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         [NullString("")]
                         [SingleColumnCollection(", ")]
                         IReadOnlyList<int?> Property,
                     );
                     """;

        var catalogs = CreateCatalogs(code, logger);
        var context = new CompatibilityContext(catalogs.EnumMemberCatalog, ["1, 42, , -7"]);

        foreach (var recordSchema in catalogs.RecordSchemaCatalog.StaticDataRecordSchemata)
        {
            foreach (var propertySchema in recordSchema.RecordPropertySchemata)
            {
                propertySchema.CheckCompatibility(context, logger);
            }
        }

        Assert.Empty(logger.Logs);
    }

    [Fact]
    public void NullableSingleColumnEnumListTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<ListTypeTests>() is not TestOutputLogger<ListTypeTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     public enum MyEnum { A, B, C }

                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         [SingleColumnCollection(", ")]
                         [NullString("")]
                         IReadOnlyList<MyEnum?> Property,
                     );
                     """;

        var catalogs = CreateCatalogs(code, logger);
        var context = new CompatibilityContext(catalogs.EnumMemberCatalog, ["C, A, "]);

        foreach (var recordSchema in catalogs.RecordSchemaCatalog.StaticDataRecordSchemata)
        {
            foreach (var propertySchema in recordSchema.RecordPropertySchemata)
            {
                propertySchema.CheckCompatibility(context, logger);
            }
        }

        Assert.Empty(logger.Logs);
    }

    [Fact]
    public void NullableSingleColumnDateTimeListTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<ListTypeTests>() is not TestOutputLogger<ListTypeTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         [SingleColumnCollection(", ")]
                         [NullString("")]
                         [DateTimeFormat("yyyy-MM-dd HH:mm:ss.fff")]
                         IReadOnlyList<DateTime?> Property,
                     );
                     """;

        var catalogs = CreateCatalogs(code, logger);
        var context = new CompatibilityContext(catalogs.EnumMemberCatalog, [", 1986-05-26 01:05:00.000, 1993-12-28 01:05:00.000"]);

        foreach (var recordSchema in catalogs.RecordSchemaCatalog.StaticDataRecordSchemata)
        {
            foreach (var propertySchema in recordSchema.RecordPropertySchemata)
            {
                propertySchema.CheckCompatibility(context, logger);
            }
        }

        Assert.Empty(logger.Logs);
    }

    [Fact]
    public void NullableSingleColumnTimeSpanListTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<ListTypeTests>() is not TestOutputLogger<ListTypeTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         [SingleColumnCollection(", ")]
                         [TimeSpanFormat("c")]
                         [NullString("")]
                         IReadOnlyList<TimeSpan?> Property,
                     );
                     """;

        var catalogs = CreateCatalogs(code, logger);
        var context = new CompatibilityContext(catalogs.EnumMemberCatalog, ["1.02:03:04.5670000, , 2.02:03:04.5670000"]);

        foreach (var recordSchema in catalogs.RecordSchemaCatalog.StaticDataRecordSchemata)
        {
            foreach (var propertySchema in recordSchema.RecordPropertySchemata)
            {
                propertySchema.CheckCompatibility(context, logger);
            }
        }

        Assert.Empty(logger.Logs);
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
            recordSchemaCatalog,
            enumMemberCatalog);
    }
}
