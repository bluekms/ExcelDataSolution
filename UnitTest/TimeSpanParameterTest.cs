using Microsoft.Extensions.Logging;
using SchemaInfoScanner;
using SchemaInfoScanner.Collectors;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.Exceptions;
using SchemaInfoScanner.Schemata;
using StaticDataAttribute;
using UnitTest.Utility;
using Xunit.Abstractions;

namespace UnitTest;

public class TimeSpanParameterTest(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void Test()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<TimeSpanParameterTest>() is not TestOutputLogger<TimeSpanParameterTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = """
                   [StaticDataRecord("TestExcel", "TestSheet")]
                   public sealed record Foo(
                       [TimeSpanFormat("c")]
                       TimeSpan Data);
                   """;

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var enumMemberContainer = new EnumMemberContainer(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);
        RecordComplianceChecker.Check(recordSchemaContainer, logger);

        var rawRecordSchema = recordSchemaContainer.StaticDataRecordSchemata[0];
        var recordSchema = RecordSchemaFactory.Create(
            rawRecordSchema,
            recordSchemaContainer,
            new Dictionary<string, int>());

        var parameter = recordSchema.RecordParameterSchemaList[0];
        var valueStr = "0.00:05";
        var arguments = Enumerable.Repeat(valueStr, 1).GetEnumerator();
        parameter.CheckCompatibility(arguments, enumMemberContainer, logger);

        Assert.Empty(logger.Logs);
    }

    [Fact]
    public void AttributeNotFoundExceptionTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<TimeSpanParameterTest>() is not TestOutputLogger<TimeSpanParameterTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = """
                   [StaticDataRecord("TestExcel", "TestSheet")]
                   public sealed record Foo(
                       TimeSpan Data);
                   """;

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);

        Assert.Throws<AttributeNotFoundException<TimeSpanFormatAttribute>>(() =>
        {
            RecordComplianceChecker.Check(recordSchemaContainer, logger);
        });

        Assert.Single(logger.Logs);
    }

    [Fact]
    public void InvalidFormatTest()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<TimeSpanParameterTest>() is not TestOutputLogger<TimeSpanParameterTest> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var code = """
                   [StaticDataRecord("TestExcel", "TestSheet")]
                   public sealed record Foo(
                       [TimeSpanFormat("c")]
                       TimeSpan Data);
                   """;

        var loadResult = RecordSchemaLoader.OnLoad(nameof(RecordTypeCheckerTest), code, logger);

        var recordSchemaCollector = new RecordSchemaCollector(loadResult);
        var enumMemberContainer = new EnumMemberContainer(loadResult);
        var recordSchemaContainer = new RecordSchemaContainer(recordSchemaCollector);
        RecordComplianceChecker.Check(recordSchemaContainer, logger);

        var rawRecordSchema = recordSchemaContainer.StaticDataRecordSchemata[0];
        var recordSchema = RecordSchemaFactory.Create(
            rawRecordSchema,
            recordSchemaContainer,
            new Dictionary<string, int>());

        Assert.Throws<FormatException>(() =>
        {
            var parameter = recordSchema.RecordParameterSchemaList[0];
            var valueStr = "01.03.2025 13:10:20,123";   // 독일
            var arguments = Enumerable.Repeat(valueStr, 1).GetEnumerator();
            parameter.CheckCompatibility(arguments, enumMemberContainer, logger);
        });
        Assert.Single(logger.Logs);
    }
}
