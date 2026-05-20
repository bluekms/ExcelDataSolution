using Microsoft.Extensions.Logging;
using Sdp.Attributes;
using Sdp.Csv;
using UnitTest.Utility;
using Xunit.Abstractions;

namespace UnitTest.CsvRecordMapperTests;

public class RegexValidationTests(ITestOutputHelper testOutputHelper)
{
    private sealed record IconRecord(
        int Id,
        [RegularExpression(@"^icons/[a-z]+\.png$")] string IconPath);

    private sealed record NullableIconRecord(
        int Id,
        [NullString("-")][RegularExpression(@"^icons/[a-z]+\.png$")] string? IconPath);

    [Theory]
    [InlineData("icons/sword.png")]
    [InlineData("icons/shield.png")]
    public void MatchingValue_Maps(string cell)
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<RegexValidationTests>() is not TestOutputLogger<RegexValidationTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var headers = new[] { "Id", "IconPath" };
        var values = new[] { "1", cell };

        var result = CsvRecordMapper.MapToRecord<IconRecord>(headers, values);

        Assert.Equal(cell, result.IconPath);
        Assert.Empty(logger.Logs);
    }

    [Theory]
    [InlineData("icons/Sword.png")]
    [InlineData("sword.png")]
    [InlineData("icons/sword.jpg")]
    public void NonMatchingValue_Throws(string cell)
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<RegexValidationTests>() is not TestOutputLogger<RegexValidationTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var headers = new[] { "Id", "IconPath" };
        var values = new[] { "1", cell };

        var ex = Assert.Throws<ArgumentException>(
            () => CsvRecordMapper.MapToRecord<IconRecord>(headers, values));
        logger.LogError(ex, ex.Message);

        Assert.Single(logger.Logs);
    }

    [Fact]
    public void NullableValueWithNullString_SkipsPattern()
    {
        var factory = new TestOutputLoggerFactory(testOutputHelper, LogLevel.Warning);
        if (factory.CreateLogger<RegexValidationTests>() is not TestOutputLogger<RegexValidationTests> logger)
        {
            throw new InvalidOperationException("Logger creation failed.");
        }

        var headers = new[] { "Id", "IconPath" };
        var values = new[] { "1", "-" };

        var result = CsvRecordMapper.MapToRecord<NullableIconRecord>(headers, values);

        Assert.Null(result.IconPath);
        Assert.Empty(logger.Logs);
    }
}
