using Microsoft.Extensions.Logging;
using SchemaInfoScanner;
using SchemaInfoScanner.Catalogs;
using SchemaInfoScanner.Collectors;
using SchemaInfoScanner.Schemata.TypedPropertySchemata;
using UnitTest.Utility;
using Xunit.Abstractions;

namespace UnitTest.PropertySchemaCompatibilityTests.CollectionPropertySchemaTests;

public class DictionaryTypeTests(ITestOutputHelper testOutputHelper)
{
    [Theory]
    [InlineData("bool", new[] { "true", "FALSE" })]
    [InlineData("byte", new[] { "0", "39", "255" })]
    [InlineData("char", new[] { "\0", "\uffff", "Z" })]
    [InlineData("decimal", new[] { "-79228162514264337593543950335", "79228162514264337593543950335", "-79,228,162,514,264,337,593,543,950,330" })]
    [InlineData("double", new[] { "-1.7976931348623157E+308", "0", "1.7976931348623157E+308" })]
    [InlineData("float", new[] { "-3.40282346638528859e+38", "-240282346638528859811704183484516925440", "86" })]
    [InlineData("int", new[] { "-2,147,483,648", "2147483647", "2,147,483,640" })]
    [InlineData("long", new[] { "-9223372036854775808", "9,223,372,036,854,775,807", "9223372036854775800" })]
    [InlineData("sbyte", new[] { "-128", "9", "127" })]
    [InlineData("short", new[] { "32,767", "-32,768", "32760" })]
    [InlineData("string", new[] { "Hello, World!", "Blue", "Kms" })]
    [InlineData("uint", new[] { "4294967295", "4,294,967,290", "0" })]
    [InlineData("ulong", new[] { "0", "18,446,744,073,709,551,615", "18446744073709551610" })]
    [InlineData("ushort", new[] { "65535", "0", "65,530" })]
    public void PrimitiveKeyDictionaryTest(string keyType, string[] keys)
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<DictionaryTypeTests>() is not TestOutputLogger<DictionaryTypeTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         Dictionary<{{keyType}}, string> Property,
                     );
                     """;

        var catalogs = CreateCatalogs(code, logger);

        var data = MakeDictionaryRawData(keys);
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

    [Theory]
    [InlineData("ushort", new[] { "65535", "0", "65,535" })]
    public void PrimitiveKeyDictionaryDuplicationFailTest(string keyType, string[] keys)
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<DictionaryTypeTests>() is not TestOutputLogger<DictionaryTypeTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         Dictionary<{{keyType}}, string> Property,
                     );
                     """;

        var catalogs = CreateCatalogs(code, logger);

        var data = MakeDictionaryRawData(keys);
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
    public void EnumKeyDictionaryTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<DictionaryTypeTests>() is not TestOutputLogger<DictionaryTypeTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     public enum MyEnum { A, a, C }

                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         Dictionary<MyEnum, string> Property,
                     );
                     """;

        var catalogs = CreateCatalogs(code, logger);

        var data = MakeDictionaryRawData(["A", "a", "C"]);
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
    public void EnumKeyDictionaryDuplicationFailTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<DictionaryTypeTests>() is not TestOutputLogger<DictionaryTypeTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     public enum MyEnum { A, a, C }

                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         Dictionary<MyEnum, string> Property,
                     );
                     """;

        var catalogs = CreateCatalogs(code, logger);

        var data = MakeDictionaryRawData(["A", "A", "C"]);
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
    public void DateTimeKeyDictionaryTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<DictionaryTypeTests>() is not TestOutputLogger<DictionaryTypeTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         [DateTimeFormat("yyyy-MM-dd HH:mm:ss.fff")]
                         Dictionary<DateTime, string> Property,
                     );
                     """;

        var catalogs = CreateCatalogs(code, logger);

        var data = MakeDictionaryRawData(["1986-05-26 01:05:00.000", "1993-12-28 01:05:00.000"]);
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
    public void TimeSpanKeyDictionaryTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<DictionaryTypeTests>() is not TestOutputLogger<DictionaryTypeTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         [TimeSpanFormat("c")]
                         Dictionary<TimeSpan, string> Property,
                     );
                     """;

        var catalogs = CreateCatalogs(code, logger);

        var data = MakeDictionaryRawData(["1.02:03:04.5670000", "2.02:03:04.5670000"]);
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

    [Theory]
    [InlineData("sbyte", new[] { "0", "1", "2", "3" }, new[] { "Hello", "", "World", "!" })]
    public void PrimitiveKeyNullablePrimitiveValueDictionaryTest(string keyType, string[] keys, string[] values)
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<DictionaryTypeTests>() is not TestOutputLogger<DictionaryTypeTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         [NullString("")]
                         ReadOnlyDictionary<{{keyType}}, string> Property,
                     );
                     """;

        var catalogs = CreateCatalogs(code, logger);

        var data = MakeDictionaryRawData(keys, values);
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

    private static string[] MakeDictionaryRawData(string[] keys)
    {
        var result = new List<string>();
        foreach (var key in keys)
        {
            result.Add(key);
            result.Add("Dummy");
        }

        return result.ToArray();
    }

    private static string[] MakeDictionaryRawData(string[] keys, string[] values)
    {
        if (keys.Length != values.Length)
        {
            throw new ArgumentException("Keys and values must have the same length.");
        }

        var result = new List<string>();
        for (var i = 0; i < keys.Length; i++)
        {
            result.Add(keys[i]);
            result.Add(values[i]);
        }

        return result.ToArray();
    }
}
