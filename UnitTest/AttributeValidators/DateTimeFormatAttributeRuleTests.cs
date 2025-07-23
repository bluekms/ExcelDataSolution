using Microsoft.Extensions.Logging;
using SchemaInfoScanner;
using SchemaInfoScanner.Catalogs;
using SchemaInfoScanner.Collectors;
using UnitTest.Utility;
using Xunit.Abstractions;

namespace UnitTest.AttributeValidators;

public class DateTimeFormatAttributeRuleTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void DateTimeTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<DateTimeFormatAttributeRuleTests>() is not TestOutputLogger<DateTimeFormatAttributeRuleTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         //[DateTimeFormat("yyyy-MM-dd HH:mm:ss.fff")]
                         DateTime Property,
                     );
                     """;

        var loadResult = RecordSchemaLoader.OnLoad(code, logger);

        var recordSchemaSet = new RecordSchemaSet(loadResult, logger);
        var recordSchemaCatalog = new RecordSchemaCatalog(recordSchemaSet);
        RecordComplianceChecker.Check(recordSchemaCatalog, logger);

        Assert.Empty(logger.Logs);
    }
}
