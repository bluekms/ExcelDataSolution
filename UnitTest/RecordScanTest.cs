using System.Reflection;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner;
using SchemaInfoScanner.Collectors;
using SchemaInfoScanner.Containers;
using UnitTest.Utility;
using Xunit.Abstractions;

namespace UnitTest;

public class RecordScanTest(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void LoadAndCheckTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<RecordScanTest>() is not TestOutputLogger<RecordScanTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var csPath = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "..",
            "..",
            "..",
            "..",
            "_TestRecord");

        var loadResults = RecordSchemaLoader.Load(csPath, logger);

        var recordSchemaCollector = new RecordSchemaSet(loadResults);
        var enumMemberCollector = new EnumDefinitionSet(loadResults);
        var semanticModelCollector = new SemanticModelSet(loadResults);

        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);
        RecordComplianceChecker.Check(recordSchemaContainer, logger);

        Assert.Empty(logger.Logs);
    }
}
