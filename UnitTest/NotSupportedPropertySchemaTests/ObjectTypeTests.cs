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

    [Fact]
    public void RecordRecordKeyAndRecordValueMapTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<ObjectTypeTests>() is not TestOutputLogger<ObjectTypeTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = $$"""
                     [StaticDataRecord("Test", "TestSheet")]
                     public sealed record MyRecord(
                         [Length(3)] FrozenDictionary<KeyData, MyData> Data
                     );

                     public record struct KeyData(InnerKey Inner, int Key1)
                     {
                         public record struct InnerKey(int X, int Y);
                     }

                     public record struct MyData([Key] KeyData Key, string Value);
                     """;

        var loadResult = RecordSchemaLoader.OnLoad(code, logger);
        Assert.Throws<NotSupportedException>(() => new RecordSchemaSet(loadResult, logger));
        Assert.Single(logger.Logs);
    }
}
