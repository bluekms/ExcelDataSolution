using Microsoft.Extensions.Logging;
using SchemaInfoScanner;
using SchemaInfoScanner.Collectors;
using SchemaInfoScanner.Containers;
using UnitTest.Utility;
using Xunit.Abstractions;

namespace UnitTest.TypedPropertySchemaTests.ContainerPropertySchemaTests;

public class HashSetTypeTests(ITestOutputHelper testOutputHelper)
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
                       HashSet<{{type}}> Property,
                   );
                   """;

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaCollector = new RecordSchemaSet(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);
        RecordComplianceChecker.Check(recordSchemaContainer, logger);

        Assert.Empty(logger.Logs);
    }

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
    public void IReadOnlyCollectionTest(string type)
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<HashSetTypeTests>() is not TestOutputLogger<HashSetTypeTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         IReadOnlyCollection<{{type}}> Property,
                     );
                     """;

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaCollector = new RecordSchemaSet(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);
        RecordComplianceChecker.Check(recordSchemaContainer, logger);

        Assert.Empty(logger.Logs);
    }

    [Theory]
    [InlineData("MyEnum")]
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
                       HashSet<{{type}}> Property,
                   );
                   """;

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaCollector = new RecordSchemaSet(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);
        RecordComplianceChecker.Check(recordSchemaContainer, logger);

        Assert.Empty(logger.Logs);
    }

    [Theory]
    [InlineData("MyEnum")]
    public void IReadOnlyCollectionEnumTest(string type)
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
                         IReadOnlyCollection<{{type}}> Property,
                     );
                     """;

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaCollector = new RecordSchemaSet(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);
        RecordComplianceChecker.Check(recordSchemaContainer, logger);

        Assert.Empty(logger.Logs);
    }

    [Theory]
    [InlineData("DateTime")]
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
                       HashSet<{{type}}> Property,
                   );
                   """;

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaCollector = new RecordSchemaSet(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);
        RecordComplianceChecker.Check(recordSchemaContainer, logger);

        Assert.Empty(logger.Logs);
    }

    [Theory]
    [InlineData("DateTime")]
    public void IReadOnlyCollectionDateTimeTest(string type)
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
                         IReadOnlyCollection<{{type}}> Property,
                     );
                     """;

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaCollector = new RecordSchemaSet(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);
        RecordComplianceChecker.Check(recordSchemaContainer, logger);

        Assert.Empty(logger.Logs);
    }

    [Theory]
    [InlineData("TimeSpan")]
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
                       HashSet<{{type}}> Property,
                   );
                   """;

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaCollector = new RecordSchemaSet(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);
        RecordComplianceChecker.Check(recordSchemaContainer, logger);

        Assert.Empty(logger.Logs);
    }

    [Theory]
    [InlineData("TimeSpan")]
    public void IReadOnlyCollectionTimeSpanTest(string type)
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
                         IReadOnlyCollection<{{type}}> Property,
                     );
                     """;

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaCollector = new RecordSchemaSet(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);
        RecordComplianceChecker.Check(recordSchemaContainer, logger);

        Assert.Empty(logger.Logs);
    }
}
