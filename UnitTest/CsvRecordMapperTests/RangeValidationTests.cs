using Microsoft.Extensions.Logging;
using Sdp.Attributes;
using Sdp.Csv;
using UnitTest.Utility;
using Xunit.Abstractions;

namespace UnitTest.CsvRecordMapperTests;

public class RangeValidationTests(ITestOutputHelper testOutputHelper)
{
    public enum Grade
    {
        Low,
        Mid,
        High,
        Extreme,
    }

    private sealed record IntRangeRecord(int Id, [Range(1, 100)] int Score);

    private sealed record EnumRangeRecord(int Id, [Range(typeof(Grade), "Low", "High")] Grade Grade);

    private sealed record StringRangeRecord(int Id, [Range(typeof(string), "apple", "zebra")] string Fruit);

    private sealed record NullableIntRangeRecord(int Id, [NullString("-")][Range(1, 100)] int? Score);

    [Theory]
    [InlineData("1", 1)]
    [InlineData("50", 50)]
    [InlineData("100", 100)]
    public void IntValueInRange_Maps(string cell, int expected)
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<RangeValidationTests>() is not TestOutputLogger<RangeValidationTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var headers = new[] { "Id", "Score" };
        var values = new[] { "1", cell };

        var result = CsvRecordMapper.MapToRecord<IntRangeRecord>(headers, values);

        Assert.Equal(expected, result.Score);
        Assert.Empty(logger.Logs);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("101")]
    public void IntValueOutOfRange_Throws(string cell)
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<RangeValidationTests>() is not TestOutputLogger<RangeValidationTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var headers = new[] { "Id", "Score" };
        var values = new[] { "1", cell };

        var ex = Assert.Throws<ArgumentOutOfRangeException>(
            () => CsvRecordMapper.MapToRecord<IntRangeRecord>(headers, values));
        logger.LogError(ex, ex.Message);

        Assert.Single(logger.Logs);
    }

    [Fact]
    public void EnumValueInRange_Maps()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<RangeValidationTests>() is not TestOutputLogger<RangeValidationTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var headers = new[] { "Id", "Grade" };
        var values = new[] { "1", "Mid" };

        var result = CsvRecordMapper.MapToRecord<EnumRangeRecord>(headers, values);

        Assert.Equal(Grade.Mid, result.Grade);
        Assert.Empty(logger.Logs);
    }

    [Fact]
    public void EnumValueOutOfRange_Throws()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<RangeValidationTests>() is not TestOutputLogger<RangeValidationTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var headers = new[] { "Id", "Grade" };
        var values = new[] { "1", "Extreme" };

        var ex = Assert.Throws<ArgumentOutOfRangeException>(
            () => CsvRecordMapper.MapToRecord<EnumRangeRecord>(headers, values));
        logger.LogError(ex, ex.Message);

        Assert.Single(logger.Logs);
    }

    [Theory]
    [InlineData("apple")]
    [InlineData("mango")]
    [InlineData("zebra")]
    public void StringValueInLexicalRange_Maps(string cell)
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<RangeValidationTests>() is not TestOutputLogger<RangeValidationTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var headers = new[] { "Id", "Fruit" };
        var values = new[] { "1", cell };

        var result = CsvRecordMapper.MapToRecord<StringRangeRecord>(headers, values);

        Assert.Equal(cell, result.Fruit);
        Assert.Empty(logger.Logs);
    }

    [Theory]
    [InlineData("ant")]
    [InlineData("zzz")]
    public void StringValueOutOfLexicalRange_Throws(string cell)
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<RangeValidationTests>() is not TestOutputLogger<RangeValidationTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var headers = new[] { "Id", "Fruit" };
        var values = new[] { "1", cell };

        var ex = Assert.Throws<ArgumentOutOfRangeException>(
            () => CsvRecordMapper.MapToRecord<StringRangeRecord>(headers, values));
        logger.LogError(ex, ex.Message);

        Assert.Single(logger.Logs);
    }

    [Fact]
    public void NullableValueWithNullString_SkipsRange()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<RangeValidationTests>() is not TestOutputLogger<RangeValidationTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var headers = new[] { "Id", "Score" };
        var values = new[] { "1", "-" };

        var result = CsvRecordMapper.MapToRecord<NullableIntRangeRecord>(headers, values);

        Assert.Null(result.Score);
        Assert.Empty(logger.Logs);
    }
}
