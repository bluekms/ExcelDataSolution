using Microsoft.Extensions.Logging;
using SchemaInfoScanner;
using SchemaInfoScanner.Catalogs;
using SchemaInfoScanner.Collectors;
using UnitTest.Utility;
using Xunit.Abstractions;

namespace UnitTest.PropertySchemaTests.CollectionPropertySchemaTests.NullableTypes;

public class HashSetTypeTests(ITestOutputHelper testOutputHelper)
{
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
    public void HashSetTest(string type)
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<HashSetTypeTests>() is not TestOutputLogger<HashSetTypeTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         [NullString("")] HashSet<{{type}}> Property,
                     );
                     """;

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

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
    public void IReadOnlySetTest(string type)
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<HashSetTypeTests>() is not TestOutputLogger<HashSetTypeTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         [NullString("")] IReadOnlySet<{{type}}> Property,
                     );
                     """;

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaSet = new RecordSchemaSet(loadResult, logger);
        var recordSchemaCatalog = new RecordSchemaCatalog(recordSchemaSet);
        RecordComplianceChecker.Check(recordSchemaCatalog, logger);

        Assert.Empty(logger.Logs);
    }

    [Theory]
    [InlineData("MyEnum?")]
    public void HashSetEnumTest(string type)
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
                       [NullString("")] HashSet<{{type}}> Property,
                   );
                   """;

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaSet = new RecordSchemaSet(loadResult, logger);
        var recordSchemaCatalog = new RecordSchemaCatalog(recordSchemaSet);
        RecordComplianceChecker.Check(recordSchemaCatalog, logger);

        Assert.Empty(logger.Logs);
    }

    [Theory]
    [InlineData("MyEnum?")]
    public void IReadOnlySetEnumTest(string type)
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
                         [NullString("")] IReadOnlySet<{{type}}> Property,
                     );
                     """;

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaSet = new RecordSchemaSet(loadResult, logger);
        var recordSchemaCatalog = new RecordSchemaCatalog(recordSchemaSet);
        RecordComplianceChecker.Check(recordSchemaCatalog, logger);

        Assert.Empty(logger.Logs);
    }

    [Theory]
    [InlineData("DateTime?")]
    public void HashSetDateTimeTest(string type)
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
                       [NullString("")]
                       HashSet<{{type}}> Property,
                   );
                   """;

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaSet = new RecordSchemaSet(loadResult, logger);
        var recordSchemaCatalog = new RecordSchemaCatalog(recordSchemaSet);
        RecordComplianceChecker.Check(recordSchemaCatalog, logger);

        Assert.Empty(logger.Logs);
    }

    [Theory]
    [InlineData("DateTime?")]
    public void IReadOnlySetDateTimeTest(string type)
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
                         [NullString("")]
                         IReadOnlySet<{{type}}> Property,
                     );
                     """;

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaSet = new RecordSchemaSet(loadResult, logger);
        var recordSchemaCatalog = new RecordSchemaCatalog(recordSchemaSet);
        RecordComplianceChecker.Check(recordSchemaCatalog, logger);

        Assert.Empty(logger.Logs);
    }

    [Theory]
    [InlineData("TimeSpan?")]
    public void HashSetTimeSpanTest(string type)
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
                       [NullString("")]
                       HashSet<{{type}}> Property,
                   );
                   """;

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaSet = new RecordSchemaSet(loadResult, logger);
        var recordSchemaCatalog = new RecordSchemaCatalog(recordSchemaSet);
        RecordComplianceChecker.Check(recordSchemaCatalog, logger);

        Assert.Empty(logger.Logs);
    }

    [Theory]
    [InlineData("TimeSpan?")]
    public void IReadOnlySetTimeSpanTest(string type)
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
                         [NullString("")]
                         IReadOnlySet<{{type}}> Property,
                     );
                     """;

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaSet = new RecordSchemaSet(loadResult, logger);
        var recordSchemaCatalog = new RecordSchemaCatalog(recordSchemaSet);
        RecordComplianceChecker.Check(recordSchemaCatalog, logger);

        Assert.Empty(logger.Logs);
    }
}
