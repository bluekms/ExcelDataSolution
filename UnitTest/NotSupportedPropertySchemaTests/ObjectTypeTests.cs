using Microsoft.Extensions.Logging;
using SchemaInfoScanner;
using SchemaInfoScanner.Collectors;
using UnitTest.Utility;
using Xunit.Abstractions;

namespace UnitTest.NotSupportedPropertySchemaTests;

public class ObjectTypeTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void RejectsObjectTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<ObjectTypeTests>() is not TestOutputLogger<ObjectTypeTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         object Property,
                     );
                     """;

        var loadResult = RecordSchemaLoader.OnLoad(code, logger);
        Assert.Throws<NotSupportedException>(() => new RecordSchemaSet(loadResult, logger));
        Assert.Single(logger.Logs);
    }

    [Fact]
    public void RejectsClassTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<ObjectTypeTests>() is not TestOutputLogger<ObjectTypeTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         MyData Property,
                     );

                     public sealed class MyData
                     {
                         public int Value { get; set; }
                     }
                     """;

        var loadResult = RecordSchemaLoader.OnLoad(code, logger);
        Assert.Throws<NotSupportedException>(() => new RecordSchemaSet(loadResult, logger));
        Assert.Single(logger.Logs);
    }

    // TODO 키가 레코드레코드, 레코드컨테이너 인 경우도 지원여부 확인할 것
}
