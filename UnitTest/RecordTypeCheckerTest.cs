using Microsoft.Extensions.Logging;
using SchemaInfoScanner;
using SchemaInfoScanner.Collectors;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.Exceptions;
using SchemaInfoScanner.TypeCheckers;
using UnitTest.Utility;
using Xunit.Abstractions;

namespace UnitTest;

public class RecordTypeCheckerTest(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void Test()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Trace);
        if (factory.CreateLogger<RecordTypeCheckerTest>() is not TestOutputLogger<RecordTypeCheckerTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = @"
            public sealed record MyRecord(
                string Name,
                int Age);";

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var enumMemberContainer = new EnumMemberContainer(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector, enumMemberContainer);

        var recordSchema = recordSchemaContainer.RecordSchemaDictionary.Values.First();

        RecordTypeChecker.Check(recordSchema, recordSchemaContainer, new(), logger);

        Assert.Empty(logger.Logs);
    }

    [Fact]
    public void IgnoreRecordTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Trace);
        if (factory.CreateLogger<RecordTypeCheckerTest>() is not TestOutputLogger<RecordTypeCheckerTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = @"
            public sealed record MyRecord(
                string Name,
                int Age);

            [Ignore]
            public sealed record SkipRecord(
                string Name,
                int Age);";

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var enumMemberContainer = new EnumMemberContainer(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector, enumMemberContainer);

        foreach (var recordSchema in recordSchemaContainer.RecordSchemaDictionary.Values)
        {
            RecordTypeChecker.Check(recordSchema, recordSchemaContainer, new(), logger);
        }

        Assert.Contains("SkipRecord is ignored.", logger.Logs);
    }

    [Fact]
    public void IgnoreParameterTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Trace);
        if (factory.CreateLogger<RecordTypeCheckerTest>() is not TestOutputLogger<RecordTypeCheckerTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = @"
            public sealed record MyRecord(
                string Name,
                [Ignore] int Age);";

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var enumMemberContainer = new EnumMemberContainer(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector, enumMemberContainer);

        var recordSchema = recordSchemaContainer.RecordSchemaDictionary.Values.First();

        foreach (var recordParameterSchema in recordSchema.RawParameterSchemaList)
        {
            SupportedTypeChecker.Check(recordParameterSchema, recordSchemaContainer, new(), logger);
        }

        Assert.Contains("MyRecord.Age is ignored.", logger.Logs);
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

        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Trace);
        if (factory.CreateLogger<RecordTypeCheckerTest>() is not TestOutputLogger<RecordTypeCheckerTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        Assert.Throws<TypeNotSupportedException>(() => SimpleCordParser.ParseAll(code, logger));
        Assert.Empty(logger.Logs);
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

        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Trace);
        if (factory.CreateLogger<RecordTypeCheckerTest>() is not TestOutputLogger<RecordTypeCheckerTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        Assert.Throws<TypeNotSupportedException>(() => SimpleCordParser.ParseAll(code, logger));
        Assert.Empty(logger.Logs);
    }
}
