using Microsoft.Extensions.Logging;
using SchemaInfoScanner;
using SchemaInfoScanner.Catalogs;
using SchemaInfoScanner.Collectors;
using SchemaInfoScanner.TypeCheckers;
using UnitTest.Utility;
using Xunit.Abstractions;

namespace UnitTest;

public class RecordTypeCheckerTest(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void Test()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<RecordTypeCheckerTest>() is not TestOutputLogger<RecordTypeCheckerTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = """
                   [StaticDataRecord("Test", "TestSheet")]
                   public sealed record MyRecord(
                       string Name,
                       int Age);
                   """;

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaSet = new RecordSchemaSet(loadResult, logger);
        var recordSchemaCatalog = new RecordSchemaCatalog(recordSchemaSet);
        RecordComplianceChecker.Check(recordSchemaCatalog, logger);

        Assert.Empty(logger.Logs);
    }

    [Fact]
    public void IgnoreRecordTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<RecordTypeCheckerTest>() is not TestOutputLogger<RecordTypeCheckerTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = """
                   [StaticDataRecord("TestExcel", "TestSheet")]
                   public sealed record MyRecord(
                       string Name,
                       int Age);

                   [Ignore]
                   public sealed record SkipRecord(
                       string Name,
                       int Age);
                   """;

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaSet = new RecordSchemaSet(loadResult, logger);
        var recordSchemaCatalog = new RecordSchemaCatalog(recordSchemaSet);
        RecordComplianceChecker.Check(recordSchemaCatalog, logger);

        Assert.Single(recordSchemaCatalog.WholeRecordSchemata);
        Assert.Empty(logger.Logs);
    }

    [Fact]
    public void IgnoreParameterTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Trace);
        if (factory.CreateLogger<RecordTypeCheckerTest>() is not TestOutputLogger<RecordTypeCheckerTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = """
                   [StaticDataRecord("Test", "TestSheet")]
                   public sealed record MyRecord(
                       string Name,
                       [Ignore] int Age);
                   """;

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaSet = new RecordSchemaSet(loadResult, logger);
        var recordSchemaCatalog = new RecordSchemaCatalog(recordSchemaSet);
        RecordComplianceChecker.Check(recordSchemaCatalog, logger);

        var recordSchema = recordSchemaCatalog.StaticDataRecordSchemata[0];
        foreach (var recordParameterSchema in recordSchema.RecordPropertySchemata)
        {
            SupportedTypeChecker.Check(recordParameterSchema, recordSchemaCatalog, [], logger);
        }

        Assert.Contains("MyRecord.Age is ignored.", logger.Logs.Select(x => x.Message));
    }

    [Fact]
    public void NotSupportedInnerStaticDataRecordTest()
    {
        var code = @"
            [StaticDataRecord(""Test"", ""TestSheet2"")]
            public sealed record Subject(
                string Name,
                List<int> QuarterScore
            );

            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyClass(
                string Name,
                Subject SubjectA,
                int Age,
            );";

        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<RecordTypeCheckerTest>() is not TestOutputLogger<RecordTypeCheckerTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        Assert.Throws<NotSupportedException>(() => SimpleCordParser.Parse(code, logger));
        Assert.Single(logger.Logs);
    }

    [Fact]
    public void NotSupportedInnerListStaticDataRecordTest()
    {
        var code = @"
            [StaticDataRecord(""Test"", ""TestSheet2"")]
            public sealed record Subject(
                string Name,
                List<int> QuarterScore
            );

            [StaticDataRecord(""Test"", ""TestSheet"")]
            public sealed record MyClass(
                string Name,
                List<Subject> SubjectA,
                int Age,
                List<Subject> SubjectB,
            );";

        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<RecordTypeCheckerTest>() is not TestOutputLogger<RecordTypeCheckerTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        Assert.Throws<NotSupportedException>(() => SimpleCordParser.Parse(code, logger));
        Assert.Single(logger.Logs);
    }
}
