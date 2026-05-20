using System.Globalization;
using Sdp.Attributes;
using Sdp.Resources;

namespace Sdp.Csv;

internal static class RangeValidator
{
    internal static void Validate(
        RangeAttribute range,
        object value,
        string columnName,
        string? dateTimeFormat,
        string? timeSpanFormat)
    {
        switch (value)
        {
            case string text:
            {
                var min = Convert.ToString(range.Minimum, CultureInfo.InvariantCulture)!;
                var max = Convert.ToString(range.Maximum, CultureInfo.InvariantCulture)!;
                if (string.CompareOrdinal(text, min) < 0 || string.CompareOrdinal(text, max) > 0)
                {
                    throw OutOfRange(columnName, text, min, max);
                }

                break;
            }

            case DateTime dateTime:
            {
                var min = DateTime.ParseExact((string)range.Minimum, dateTimeFormat!, CultureInfo.InvariantCulture);
                var max = DateTime.ParseExact((string)range.Maximum, dateTimeFormat!, CultureInfo.InvariantCulture);
                if (dateTime < min || dateTime > max)
                {
                    throw OutOfRange(columnName, dateTime, min, max);
                }

                break;
            }

            case TimeSpan timeSpan:
            {
                var min = TimeSpan.ParseExact((string)range.Minimum, timeSpanFormat!, CultureInfo.InvariantCulture);
                var max = TimeSpan.ParseExact((string)range.Maximum, timeSpanFormat!, CultureInfo.InvariantCulture);
                if (timeSpan < min || timeSpan > max)
                {
                    throw OutOfRange(columnName, timeSpan, min, max);
                }

                break;
            }

            case Enum enumValue:
            {
                var enumType = enumValue.GetType();
                var actual = Convert.ToInt64(enumValue, CultureInfo.InvariantCulture);
                var min = ParseEnumBound(range.Minimum, enumType);
                var max = ParseEnumBound(range.Maximum, enumType);
                if (actual < min || actual > max)
                {
                    throw OutOfRange(columnName, enumValue, range.Minimum, range.Maximum);
                }

                break;
            }

            default:
            {
                var comparable = (IComparable)value;
                var min = Convert.ChangeType(range.Minimum, value.GetType(), CultureInfo.InvariantCulture);
                var max = Convert.ChangeType(range.Maximum, value.GetType(), CultureInfo.InvariantCulture);
                if (comparable.CompareTo(min) < 0 || comparable.CompareTo(max) > 0)
                {
                    throw OutOfRange(columnName, value, min, max);
                }

                break;
            }
        }
    }

    private static long ParseEnumBound(object bound, Type enumType)
    {
        if (bound is string text)
        {
            if (long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var asLong))
            {
                return asLong;
            }

            return Convert.ToInt64(Enum.Parse(enumType, text), CultureInfo.InvariantCulture);
        }

        return Convert.ToInt64(bound, CultureInfo.InvariantCulture);
    }

    private static ArgumentOutOfRangeException OutOfRange(
        string columnName,
        object value,
        object min,
        object max)
    {
        return new ArgumentOutOfRangeException(
            columnName,
            value,
            string.Format(
                CultureInfo.CurrentCulture,
                Messages.Composite.ValueOutOfRange,
                columnName,
                value,
                min,
                max));
    }
}
