using System.Collections.Frozen;
using System.Collections.Immutable;
using Sdp.Attributes;
using Sdp.Csv;

namespace UnitTest.CsvRecordMapperTests;

public class DateTimeAndTimeSpanFormatMapperTests
{
    public sealed record DateTimeRecord(
        int Id,
        [DateTimeFormat("yyyy-MM-dd")] DateTime Date);

    [Fact]
    public void DateTime_StandardIsoFormat_Parses()
    {
        var headers = new[] { "Id", "Date" };
        var values = new[] { "1", "2026-05-19" };

        var result = CsvRecordMapper.MapToRecord<DateTimeRecord>(headers, values);

        Assert.Equal(new DateTime(2026, 5, 19), result.Date);
    }

    public sealed record DateTimeWithoutFormatRecord(int Id, DateTime Date);

    [Fact]
    public void DateTime_WithoutFormatAttribute_Throws()
    {
        var headers = new[] { "Id", "Date" };
        var values = new[] { "1", "2026-05-19" };

        Assert.Throws<ArgumentNullException>(() =>
            CsvRecordMapper.MapToRecord<DateTimeWithoutFormatRecord>(headers, values));
    }

    public sealed record TimeSpanWithoutFormatRecord(int Id, TimeSpan Cooldown);

    [Fact]
    public void TimeSpan_WithoutFormatAttribute_Throws()
    {
        var headers = new[] { "Id", "Cooldown" };
        var values = new[] { "1", "01:00:00" };

        Assert.Throws<ArgumentNullException>(() =>
            CsvRecordMapper.MapToRecord<TimeSpanWithoutFormatRecord>(headers, values));
    }

    public sealed record DateTimeUsFormatRecord(
        int Id,
        [DateTimeFormat("MM/dd/yyyy")] DateTime Date);

    [Fact]
    public void DateTime_NonIsoFormat_ParsesWithExactFormat()
    {
        var headers = new[] { "Id", "Date" };
        var values = new[] { "1", "05/19/2026" };

        var result = CsvRecordMapper.MapToRecord<DateTimeUsFormatRecord>(headers, values);

        Assert.Equal(new DateTime(2026, 5, 19), result.Date);
    }

    [Fact]
    public void DateTime_ValueMismatchFormat_Throws()
    {
        var headers = new[] { "Id", "Date" };
        var values = new[] { "1", "2026-05-19" };

        Assert.Throws<FormatException>(() =>
            CsvRecordMapper.MapToRecord<DateTimeUsFormatRecord>(headers, values));
    }

    public sealed record NullableDateTimeRecord(
        int Id,
        [DateTimeFormat("yyyy-MM-dd")][NullString("NULL")] DateTime? Date);

    [Fact]
    public void NullableDateTime_NullString_ReturnsNull()
    {
        var headers = new[] { "Id", "Date" };
        var values = new[] { "1", "NULL" };

        var result = CsvRecordMapper.MapToRecord<NullableDateTimeRecord>(headers, values);

        Assert.Null(result.Date);
    }

    [Fact]
    public void NullableDateTime_Value_ParsesWithFormat()
    {
        var headers = new[] { "Id", "Date" };
        var values = new[] { "1", "2026-05-19" };

        var result = CsvRecordMapper.MapToRecord<NullableDateTimeRecord>(headers, values);

        Assert.Equal(new DateTime(2026, 5, 19), result.Date);
    }

    public sealed record TimeSpanRecord(
        int Id,
        [TimeSpanFormat(@"hh\:mm\:ss")] TimeSpan Cooldown);

    [Fact]
    public void TimeSpan_WithFormat_Parses()
    {
        var headers = new[] { "Id", "Cooldown" };
        var values = new[] { "1", "01:30:00" };

        var result = CsvRecordMapper.MapToRecord<TimeSpanRecord>(headers, values);

        Assert.Equal(TimeSpan.FromMinutes(90), result.Cooldown);
    }

    [Fact]
    public void TimeSpan_ValueMismatchFormat_Throws()
    {
        var headers = new[] { "Id", "Cooldown" };
        var values = new[] { "1", "90m" };

        Assert.Throws<FormatException>(() =>
            CsvRecordMapper.MapToRecord<TimeSpanRecord>(headers, values));
    }

    public sealed record NullableTimeSpanRecord(
        int Id,
        [TimeSpanFormat(@"hh\:mm\:ss")][NullString("-")] TimeSpan? Cooldown);

    [Fact]
    public void NullableTimeSpan_NullString_ReturnsNull()
    {
        var headers = new[] { "Id", "Cooldown" };
        var values = new[] { "1", "-" };

        var result = CsvRecordMapper.MapToRecord<NullableTimeSpanRecord>(headers, values);

        Assert.Null(result.Cooldown);
    }

    public sealed record DateTimeArrayRecord(
        int Id,
        [DateTimeFormat("yyyy-MM-dd")]
        [Length(2)] ImmutableArray<DateTime> Period);

    [Fact]
    public void ImmutableArray_DateTimeElements_AppliesFormatPerElement()
    {
        var headers = new[] { "Id", "Period[0]", "Period[1]" };
        var values = new[] { "1", "2026-05-01", "2026-05-31" };

        var result = CsvRecordMapper.MapToRecord<DateTimeArrayRecord>(headers, values);

        Assert.Equal(new DateTime(2026, 5, 1), result.Period[0]);
        Assert.Equal(new DateTime(2026, 5, 31), result.Period[1]);
    }

    public sealed record TimeSpanArrayRecord(
        int Id,
        [TimeSpanFormat(@"hh\:mm\:ss")]
        [Length(2)] ImmutableArray<TimeSpan> Cooldowns);

    [Fact]
    public void ImmutableArray_TimeSpanElements_AppliesFormatPerElement()
    {
        var headers = new[] { "Id", "Cooldowns[0]", "Cooldowns[1]" };
        var values = new[] { "1", "00:00:30", "01:00:00" };

        var result = CsvRecordMapper.MapToRecord<TimeSpanArrayRecord>(headers, values);

        Assert.Equal(TimeSpan.FromSeconds(30), result.Cooldowns[0]);
        Assert.Equal(TimeSpan.FromHours(1), result.Cooldowns[1]);
    }

    public sealed record SingleColumnDateTimeRecord(
        int Id,
        [DateTimeFormat("yyyy-MM-dd")]
        [SingleColumnCollection("|")] ImmutableArray<DateTime> Dates);

    [Fact]
    public void SingleColumnCollection_DateTimeElements_AppliesFormat()
    {
        var headers = new[] { "Id", "Dates" };
        var values = new[] { "1", "2026-05-01|2026-05-02|2026-05-03" };

        var result = CsvRecordMapper.MapToRecord<SingleColumnDateTimeRecord>(headers, values);

        Assert.Equal(3, result.Dates.Length);
        Assert.Equal(new DateTime(2026, 5, 1), result.Dates[0]);
        Assert.Equal(new DateTime(2026, 5, 3), result.Dates[2]);
    }

    public sealed record FrozenSetDateTimeRecord(
        int Id,
        [DateTimeFormat("yyyy-MM-dd")]
        [Length(2)] FrozenSet<DateTime> Dates);

    [Fact]
    public void FrozenSet_DateTimeElements_AppliesFormat()
    {
        var headers = new[] { "Id", "Dates[0]", "Dates[1]" };
        var values = new[] { "1", "2026-05-01", "2026-05-02" };

        var result = CsvRecordMapper.MapToRecord<FrozenSetDateTimeRecord>(headers, values);

        Assert.Equal(2, result.Dates.Count);
        Assert.Contains(new DateTime(2026, 5, 1), (IEnumerable<DateTime>)result.Dates);
        Assert.Contains(new DateTime(2026, 5, 2), (IEnumerable<DateTime>)result.Dates);
    }
}
