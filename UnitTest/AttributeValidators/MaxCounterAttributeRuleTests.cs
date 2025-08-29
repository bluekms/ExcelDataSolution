using Microsoft.Extensions.Logging;
using SchemaInfoScanner;
using SchemaInfoScanner.Catalogs;
using SchemaInfoScanner.Collectors;
using SchemaInfoScanner.Exceptions;
using UnitTest.Utility;
using Xunit.Abstractions;

namespace UnitTest.AttributeValidators;

public class MaxCounterAttributeRuleTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void CanUseArrayTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<MaxCounterAttributeRuleTests>() is not TestOutputLogger<MaxCounterAttributeRuleTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         [MaxCount(3)]
                         ImmutableArray<int> Property,
                     );
                     """;

        var loadResult = RecordSchemaLoader.OnLoad(code, logger);
        var recordSchemaSet = new RecordSchemaSet(loadResult, logger);
        var recordSchemaCatalog = new RecordSchemaCatalog(recordSchemaSet);

        RecordComplianceChecker.Check(recordSchemaCatalog, logger);
        Assert.Empty(logger.Logs);
    }

    [Fact]
    public void CanUseSetTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<MaxCounterAttributeRuleTests>() is not TestOutputLogger<MaxCounterAttributeRuleTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         [MaxCount(3)]
                         FrozenSet<int> Property,
                     );
                     """;

        var loadResult = RecordSchemaLoader.OnLoad(code, logger);
        var recordSchemaSet = new RecordSchemaSet(loadResult, logger);
        var recordSchemaCatalog = new RecordSchemaCatalog(recordSchemaSet);

        RecordComplianceChecker.Check(recordSchemaCatalog, logger);
        Assert.Empty(logger.Logs);
    }

    [Fact]
    public void CanUseDictionaryTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<MaxCounterAttributeRuleTests>() is not TestOutputLogger<MaxCounterAttributeRuleTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         [MaxCount(3)]
                         FrozenDictionary<int, string> Property,
                     );
                     """;

        var loadResult = RecordSchemaLoader.OnLoad(code, logger);
        var recordSchemaSet = new RecordSchemaSet(loadResult, logger);
        var recordSchemaCatalog = new RecordSchemaCatalog(recordSchemaSet);

        RecordComplianceChecker.Check(recordSchemaCatalog, logger);
        Assert.Empty(logger.Logs);
    }

    [Fact]
    public void DisallowTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<MaxCounterAttributeRuleTests>() is not TestOutputLogger<MaxCounterAttributeRuleTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         [MaxCount(3)]
                         int Property,
                     );
                     """;

        var loadResult = RecordSchemaLoader.OnLoad(code, logger);
        var recordSchemaSet = new RecordSchemaSet(loadResult, logger);
        var recordSchemaCatalog = new RecordSchemaCatalog(recordSchemaSet);

        Assert.Throws<InvalidAttributeUsageException>(() => RecordComplianceChecker.Check(recordSchemaCatalog, logger));
        Assert.Single(logger.Logs);
    }
}
