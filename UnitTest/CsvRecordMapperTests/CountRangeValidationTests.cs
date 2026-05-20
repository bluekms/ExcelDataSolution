using System.Collections.Frozen;
using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using Sdp.Attributes;
using Sdp.Csv;
using UnitTest.Utility;
using Xunit.Abstractions;

namespace UnitTest.CsvRecordMapperTests;

public class CountRangeValidationTests(ITestOutputHelper testOutputHelper)
{
    private sealed record TagsRecord(
        int Id,
        [SingleColumnCollection(",")][CountRange(2, 4)] ImmutableArray<string> Tags);

    [Theory]
    [InlineData("a,b", 2)]
    [InlineData("a,b,c", 3)]
    [InlineData("a,b,c,d", 4)]
    public void CountWithinRange_Maps(string cell, int expectedCount)
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<CountRangeValidationTests>() is not TestOutputLogger<CountRangeValidationTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var headers = new[] { "Id", "Tags" };
        var values = new[] { "1", cell };

        var result = CsvRecordMapper.MapToRecord<TagsRecord>(headers, values);

        Assert.Equal(expectedCount, result.Tags.Length);
        Assert.Empty(logger.Logs);
    }

    [Fact]
    public void CountBelowMin_Throws()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<CountRangeValidationTests>() is not TestOutputLogger<CountRangeValidationTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var headers = new[] { "Id", "Tags" };
        var values = new[] { "1", "a" };

        var ex = Assert.Throws<ArgumentException>(
            () => CsvRecordMapper.MapToRecord<TagsRecord>(headers, values));
        logger.LogError(ex, ex.Message);

        Assert.Single(logger.Logs);
    }

    [Fact]
    public void CountAboveMax_Throws()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<CountRangeValidationTests>() is not TestOutputLogger<CountRangeValidationTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var headers = new[] { "Id", "Tags" };
        var values = new[] { "1", "a,b,c,d,e" };

        var ex = Assert.Throws<ArgumentException>(
            () => CsvRecordMapper.MapToRecord<TagsRecord>(headers, values));
        logger.LogError(ex, ex.Message);

        Assert.Single(logger.Logs);
    }

    private sealed record TagSetRecord(
        int Id,
        [SingleColumnCollection(",")][CountRange(2, 4)] FrozenSet<string> Tags);

    [Theory]
    [InlineData("a,b", 2)]
    [InlineData("a,b,c", 3)]
    [InlineData("a,b,c,d", 4)]
    public void FrozenSetCountWithinRange_Maps(string cell, int expectedCount)
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<CountRangeValidationTests>() is not TestOutputLogger<CountRangeValidationTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var headers = new[] { "Id", "Tags" };
        var values = new[] { "1", cell };

        var result = CsvRecordMapper.MapToRecord<TagSetRecord>(headers, values);

        Assert.Equal(expectedCount, result.Tags.Count);
        Assert.Empty(logger.Logs);
    }

    [Fact]
    public void FrozenSetWithDuplicate_Throws()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<CountRangeValidationTests>() is not TestOutputLogger<CountRangeValidationTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var headers = new[] { "Id", "Tags" };
        var values = new[] { "1", "a,b,a" };

        var ex = Assert.Throws<ArgumentException>(
            () => CsvRecordMapper.MapToRecord<TagSetRecord>(headers, values));
        logger.LogError(ex, ex.Message);

        Assert.Single(logger.Logs);
    }
}
