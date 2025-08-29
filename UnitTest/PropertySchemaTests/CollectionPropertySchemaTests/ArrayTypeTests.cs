using Microsoft.Extensions.Logging;
using SchemaInfoScanner;
using SchemaInfoScanner.Catalogs;
using SchemaInfoScanner.Collectors;
using UnitTest.Utility;
using Xunit.Abstractions;

namespace UnitTest.PropertySchemaTests.CollectionPropertySchemaTests;

public class ArrayTypeTests(ITestOutputHelper testOutputHelper)
{
    [Theory]
    [InlineData("bool")]
    [InlineData("byte")]
    [InlineData("char")]
    [InlineData("decimal")]
    [InlineData("double")]
    [InlineData("float")]
    [InlineData("int")]
    [InlineData("long")]
    [InlineData("sbyte")]
    [InlineData("short")]
    [InlineData("string")]
    [InlineData("uint")]
    [InlineData("ulong")]
    [InlineData("ushort")]
    public void ArrayTest(string type)
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<ArrayTypeTests>() is not TestOutputLogger<ArrayTypeTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                   [StaticDataRecord("Test", "TestSheet")]
                   public sealed record MyRecord(
                       ImmutableArray<{{type}}> Property,
                   );
                   """;

        var loadResult = RecordSchemaLoader.OnLoad(code, logger);

        var recordSchemaSet = new RecordSchemaSet(loadResult, logger);
        var recordSchemaCatalog = new RecordSchemaCatalog(recordSchemaSet);
        RecordComplianceChecker.Check(recordSchemaCatalog, logger);

        Assert.Empty(logger.Logs);
    }

    [Theory]
    [InlineData("bool?")]
    [InlineData("byte?")]
    [InlineData("char?")]
    [InlineData("decimal?")]
    [InlineData("double?")]
    [InlineData("float?")]
    [InlineData("int?")]
    [InlineData("long?")]
    [InlineData("sbyte?")]
    [InlineData("short?")]
    [InlineData("string?")]
    [InlineData("uint?")]
    [InlineData("ulong?")]
    [InlineData("ushort?")]
    public void NullableArrayTest(string type)
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<ArrayTypeTests>() is not TestOutputLogger<ArrayTypeTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         ImmutableArray<{{type}}> Property,
                     );
                     """;

        var loadResult = RecordSchemaLoader.OnLoad(code, logger);

        var recordSchemaSet = new RecordSchemaSet(loadResult, logger);
        var recordSchemaCatalog = new RecordSchemaCatalog(recordSchemaSet);
        RecordComplianceChecker.Check(recordSchemaCatalog, logger);

        Assert.Empty(logger.Logs);
    }

    [Theory]
    [InlineData("MyEnum")]
    public void EnumArrayTest(string type)
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
                       ImmutableArray<{{type}}> Property,
                   );
                   """;

        var loadResult = RecordSchemaLoader.OnLoad(code, logger);

        var recordSchemaSet = new RecordSchemaSet(loadResult, logger);
        var recordSchemaCatalog = new RecordSchemaCatalog(recordSchemaSet);
        RecordComplianceChecker.Check(recordSchemaCatalog, logger);

        Assert.Empty(logger.Logs);
    }

    [Theory]
    [InlineData("DateTime")]
    [InlineData("DateTime?")] // 이건 실패해야 함
    public void DateTimeArrayTest(string type)
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
                       ImmutableArray<{{type}}> Property,
                   );
                   """;

        var loadResult = RecordSchemaLoader.OnLoad(code, logger);

        var recordSchemaSet = new RecordSchemaSet(loadResult, logger);
        var recordSchemaCatalog = new RecordSchemaCatalog(recordSchemaSet);
        RecordComplianceChecker.Check(recordSchemaCatalog, logger);

        Assert.Empty(logger.Logs);
    }

    [Theory]
    [InlineData("TimeSpan")]
    public void TimeSpanArrayTest(string type)
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
                       ImmutableArray<{{type}}> Property,
                   );
                   """;

        var loadResult = RecordSchemaLoader.OnLoad(code, logger);

        var recordSchemaSet = new RecordSchemaSet(loadResult, logger);
        var recordSchemaCatalog = new RecordSchemaCatalog(recordSchemaSet);
        RecordComplianceChecker.Check(recordSchemaCatalog, logger);

        Assert.Empty(logger.Logs);
    }
}
