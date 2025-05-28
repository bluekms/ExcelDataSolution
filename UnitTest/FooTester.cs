using Microsoft.Extensions.Logging;
using SchemaInfoScanner;
using SchemaInfoScanner.Collectors;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.TypeCheckers;
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

        var recordSchemaCollector = new RecordSchemaSet(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);
        RecordComplianceChecker.Check(recordSchemaContainer, logger);

        var recordSchema = recordSchemaContainer.StaticDataRecordSchemata[0];
        foreach (var parameterSchema in recordSchema.RecordParameterSchemaList)
        {
            PrimitiveTypeChecker.Check(parameterSchema);
        }

        Assert.Empty(logger.Logs);
    }
}
