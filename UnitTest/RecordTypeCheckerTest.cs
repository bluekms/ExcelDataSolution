using Microsoft.Extensions.Logging;
using SchemaInfoScanner;
using SchemaInfoScanner.Collectors;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.TypeCheckers;
using Xunit.Abstractions;

namespace UnitTest;

public class RecordTypeCheckerTest
{
    private readonly ITestOutputHelper testOutputHelper;

    public RecordTypeCheckerTest(ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void Test()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordScanTest>();

        var code = @"
            public sealed record MyRecord(
                string Name,
                int Age);";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary.Values.First();

        RecordTypeChecker.Check(recordSchema, recordSchemaContainer, new(), logger);
    }
}
