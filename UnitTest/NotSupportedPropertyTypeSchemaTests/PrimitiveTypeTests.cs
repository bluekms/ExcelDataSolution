using Microsoft.Extensions.Logging;
using SchemaInfoScanner;
using SchemaInfoScanner.Collectors;
using UnitTest.Utility;
using Xunit.Abstractions;

namespace UnitTest.NotSupportedPropertyTypeTests;

public class PrimitiveTypeTests(ITestOutputHelper testOutputHelper)
{
    [Theory]
    [InlineData("nint")]
    [InlineData("nuint")]
    public void RejectsPrimitiveTest(string type)
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<PrimitiveTypeTests>() is not TestOutputLogger<PrimitiveTypeTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         {{type}} Property,
                     );
                     """;

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);
        Assert.Throws<NotSupportedException>(() => new RecordSchemaSet(loadResult, logger));
        Assert.Single(logger.Logs);
    }
}
