using Microsoft.Extensions.Logging;
using SchemaInfoScanner;
using SchemaInfoScanner.Collectors;
using SchemaInfoScanner.Containers;
using UnitTest.Utility;
using Xunit.Abstractions;

namespace UnitTest;

public class FooTester(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void Test()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<FooTester>() is not TestOutputLogger<FooTester> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = """
                   [StaticDataRecord("Test", "TestSheet")]
                   public sealed record MyRecord(
                       Dictionary<int, string> Data,
                   );
                   """;

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        var recordSchemaSet = new RecordSchemaSet(loadResult, logger);

        var recordSchemaCatalog = new RecordSchemaCatalog(recordSchemaSet);
        RecordComplianceChecker.Check(recordSchemaCatalog, logger);

        foreach (var recordSchema in recordSchemaCatalog.StaticDataRecordSchemata)
        {
            foreach (var propertySchema in recordSchema.RecordPropertySchemata)
            {
            }
        }

        Assert.Empty(logger.Logs);
    }
}
