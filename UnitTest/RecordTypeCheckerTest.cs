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
        var logger = factory.CreateLogger<RecordTypeCheckerTest>();

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

    [Fact]
    public void IgnoreRecordTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordTypeCheckerTest>();
        if (logger is not TestOutputLogger<RecordTypeCheckerTest> testOutputLogger)
        {
            throw new InvalidOperationException("Logger is not TestOutputLogger<RecordTypeCheckerTest>.");
        }

        var code = @"
            public sealed record MyRecord(
                string Name,
                int Age);

            [Ignore]
            public sealed record SkipRecord(
                string Name,
                int Age);";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        foreach (var recordSchema in recordSchemaContainer.RecordSchemaDictionary.Values)
        {
            RecordTypeChecker.Check(recordSchema, recordSchemaContainer, new(), logger);
        }

        Assert.Contains(".SkipRecord is ignored.", testOutputLogger.Logs);
    }

    [Fact]
    public void IgnoreParameterTest()
    {
        var factory = new TestOutputLoggerFactory(this.testOutputHelper, LogLevel.Trace);
        var logger = factory.CreateLogger<RecordTypeCheckerTest>();
        if (logger is not TestOutputLogger<RecordTypeCheckerTest> testOutputLogger)
        {
            throw new InvalidOperationException("Logger is not TestOutputLogger<RecordTypeCheckerTest>.");
        }

        var code = @"
            public sealed record MyRecord(
                string Name,
                [Ignore] int Age);";

        var loadResult = Loader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary.Values.First();

        foreach (var recordParameterSchema in recordSchema.RecordParameterSchemaList)
        {
            SupportedTypeChecker.Check(recordParameterSchema, recordSchemaContainer, new(), logger);
        }

        Assert.Contains(".MyRecord.Age is ignored.", testOutputLogger.Logs);
    }
}
