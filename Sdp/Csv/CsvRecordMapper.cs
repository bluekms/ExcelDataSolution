using System.Collections.Frozen;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using Sdp.Attributes;
using Sdp.Resources;

namespace Sdp.Csv;

internal static class CsvRecordMapper
{
    public static object MapToRecord(Type recordType, string[] headers, string[] values)
    {
        var typeInfo = CsvTypeCache.GetTypeInfo(recordType);
        var headerIndexMap = BuildHeaderIndexMap(headers);
        var args = new object?[typeInfo.Parameters.Length];

        for (var i = 0; i < typeInfo.Parameters.Length; i++)
        {
            var paramInfo = typeInfo.Parameters[i];
            args[i] = ConvertValue(paramInfo, paramInfo.ColumnName, headerIndexMap, values);
        }

        return typeInfo.Constructor.Invoke(args)!;
    }

    public static TRecord MapToRecord<TRecord>(string[] headers, string[] values)
        where TRecord : notnull
        => (TRecord)MapToRecord(typeof(TRecord), headers, values);

    private static Dictionary<string, int> BuildHeaderIndexMap(string[] headers)
    {
        var map = new Dictionary<string, int>(headers.Length);
        for (var i = 0; i < headers.Length; i++)
        {
            map[headers[i]] = i;
        }

        return map;
    }

    private static object? ConvertValue(
        ParameterMappingInfo paramInfo,
        string baseName,
        Dictionary<string, int> headerIndexMap,
        string[] values)
    {
        if (paramInfo.CollectionKind != CollectionKind.None)
        {
            return paramInfo.CollectionKind switch
            {
                CollectionKind.ImmutableArray => ConvertToImmutableArray(
                    paramInfo.ElementType!,
                    baseName,
                    paramInfo.Length!.Value,
                    headerIndexMap,
                    values,
                    paramInfo.NullString,
                    paramInfo.DateTimeFormat,
                    paramInfo.TimeSpanFormat),
                CollectionKind.FrozenSet => ConvertToFrozenSet(
                    paramInfo.ElementType!,
                    baseName,
                    paramInfo.Length!.Value,
                    headerIndexMap,
                    values,
                    paramInfo.NullString,
                    paramInfo.DateTimeFormat,
                    paramInfo.TimeSpanFormat),
                CollectionKind.FrozenDictionary => ConvertToFrozenDictionary(
                    paramInfo.KeyType!,
                    paramInfo.ElementType!,
                    baseName,
                    paramInfo.Length!.Value,
                    headerIndexMap,
                    values,
                    paramInfo.NullString),
                CollectionKind.SingleColumnImmutableArray => ConvertToSingleColumnImmutableArray(
                    paramInfo.ElementType!,
                    baseName,
                    paramInfo.SingleColumnSeparator!,
                    headerIndexMap,
                    values,
                    paramInfo.NullString,
                    paramInfo.CountRange,
                    paramInfo.DateTimeFormat,
                    paramInfo.TimeSpanFormat),
                CollectionKind.SingleColumnFrozenSet => ConvertToSingleColumnFrozenSet(
                    paramInfo.ElementType!,
                    baseName,
                    paramInfo.SingleColumnSeparator!,
                    headerIndexMap,
                    values,
                    paramInfo.NullString,
                    paramInfo.CountRange,
                    paramInfo.DateTimeFormat,
                    paramInfo.TimeSpanFormat),
                _ => throw new InvalidOperationException(string.Format(
                    CultureInfo.CurrentCulture,
                    Messages.Composite.UnknownCollectionType,
                    paramInfo.CollectionKind)),
            };
        }

        if (!IsPrimitiveOrSimpleType(paramInfo.ParameterType))
        {
            return CreateRecordInstance(
                paramInfo.ParameterType,
                baseName,
                headerIndexMap,
                values,
                paramInfo.NullString);
        }

        if (!headerIndexMap.TryGetValue(baseName, out var index))
        {
            throw new InvalidOperationException(string.Format(
                CultureInfo.CurrentCulture,
                Messages.Composite.CsvHeaderNotFound,
                baseName));
        }

        var convertedValue = ConvertStringValue(
            paramInfo.ParameterType,
            values[index],
            paramInfo.NullString,
            paramInfo.IsKey,
            paramInfo.DateTimeFormat,
            paramInfo.TimeSpanFormat);

        ValidateScalar(paramInfo, convertedValue);
        return convertedValue;
    }

    private static void ValidateScalar(ParameterMappingInfo paramInfo, object? value)
    {
        if (value is null)
        {
            return;
        }

        if (paramInfo.Range is not null)
        {
            RangeValidator.Validate(
                paramInfo.Range,
                value,
                paramInfo.ColumnName,
                paramInfo.DateTimeFormat,
                paramInfo.TimeSpanFormat);
        }

        if (paramInfo.Pattern is not null && value is string text)
        {
            ValidatePattern(paramInfo.Pattern, text, paramInfo.ColumnName);
        }
    }

    private static void ValidatePattern(Regex pattern, string value, string columnName)
    {
        if (pattern.IsMatch(value))
        {
            return;
        }

        throw new ArgumentException(string.Format(
            CultureInfo.CurrentCulture,
            Messages.Composite.PatternMismatch,
            columnName,
            value,
            pattern.ToString()));
    }

    private static object ConvertStringValue(
        Type targetType,
        string value,
        string? nullString,
        bool skipEnumDefinedCheck = false,
        string? dateTimeFormat = null,
        string? timeSpanFormat = null)
    {
        var underlyingType = Nullable.GetUnderlyingType(targetType);
        if (underlyingType is not null)
        {
            if (nullString is not null && value == nullString)
            {
                return null!;
            }

            targetType = underlyingType;
        }

        if (targetType == typeof(string))
        {
            if (nullString is not null && value == nullString)
            {
                return null!;
            }

            return value;
        }

        if (targetType.IsEnum)
        {
            var parsed = Enum.Parse(targetType, value);
            if (!skipEnumDefinedCheck && !Enum.IsDefined(targetType, parsed))
            {
                throw new ArgumentException(string.Format(
                    CultureInfo.CurrentCulture,
                    Messages.Composite.EnumValueNotDefined,
                    value,
                    targetType.Name));
            }

            return parsed;
        }

        if (targetType == typeof(DateTime))
        {
            return DateTime.ParseExact(value, dateTimeFormat!, CultureInfo.InvariantCulture);
        }

        if (targetType == typeof(TimeSpan))
        {
            return TimeSpan.ParseExact(value, timeSpanFormat!, CultureInfo.InvariantCulture);
        }

        return Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
    }

    private static object? ConvertToImmutableArray(
        Type elementType,
        string baseName,
        int length,
        Dictionary<string, int> headerIndexMap,
        string[] values,
        string? nullString,
        string? dateTimeFormat = null,
        string? timeSpanFormat = null)
    {
        var array = Array.CreateInstance(elementType, length);
        var isPrimitive = IsPrimitiveOrSimpleType(elementType);

        for (var i = 0; i < length; i++)
        {
            var headerName = $"{baseName}[{i}]";
            object? convertedValue;

            if (isPrimitive)
            {
                if (!headerIndexMap.TryGetValue(headerName, out var index))
                {
                    throw new InvalidOperationException(string.Format(
                        CultureInfo.CurrentCulture,
                        Messages.Composite.CsvHeaderNotFound,
                        headerName));
                }

                convertedValue = ConvertStringValue(
                    elementType,
                    values[index],
                    nullString,
                    dateTimeFormat: dateTimeFormat,
                    timeSpanFormat: timeSpanFormat);
            }
            else
            {
                convertedValue = CreateRecordInstance(elementType, headerName, headerIndexMap, values, nullString);
            }

            array.SetValue(convertedValue, i);
        }

        var createMethod = CsvTypeCache.GetImmutableArrayCreateMethod(elementType);
        return createMethod.Invoke(null, [array]);
    }

    private static string[] SplitSingleColumnCell(
        string baseName,
        string separator,
        Dictionary<string, int> headerIndexMap,
        string[] values)
    {
        if (!headerIndexMap.TryGetValue(baseName, out var index))
        {
            throw new InvalidOperationException(string.Format(
                CultureInfo.CurrentCulture,
                Messages.Composite.CsvHeaderNotFound,
                baseName));
        }

        return values[index].Split(separator);
    }

    private static void ValidateCountRange(
        string baseName,
        int count,
        CountRangeAttribute? countRange)
    {
        if (countRange is null)
        {
            return;
        }

        if (count < countRange.MinCount || count > countRange.MaxCount)
        {
            throw new ArgumentException(string.Format(
                CultureInfo.CurrentCulture,
                Messages.Composite.CountOutOfRange,
                baseName,
                count,
                countRange.MinCount,
                countRange.MaxCount));
        }
    }

    private static object? ConvertToSingleColumnImmutableArray(
        Type elementType,
        string baseName,
        string separator,
        Dictionary<string, int> headerIndexMap,
        string[] values,
        string? nullString,
        CountRangeAttribute? countRange,
        string? dateTimeFormat = null,
        string? timeSpanFormat = null)
    {
        var parts = SplitSingleColumnCell(baseName, separator, headerIndexMap, values);
        ValidateCountRange(baseName, parts.Length, countRange);

        var array = Array.CreateInstance(elementType, parts.Length);

        for (var i = 0; i < parts.Length; i++)
        {
            var trimmedValue = parts[i].Trim();
            var convertedValue = ConvertStringValue(
                elementType,
                trimmedValue,
                nullString,
                dateTimeFormat: dateTimeFormat,
                timeSpanFormat: timeSpanFormat);
            array.SetValue(convertedValue, i);
        }

        var createMethod = CsvTypeCache.GetImmutableArrayCreateMethod(elementType);
        return createMethod.Invoke(null, [array]);
    }

    private static object? InvokeHelper(MethodInfo helperMethod, object?[] arguments)
    {
        try
        {
            return helperMethod.Invoke(null, arguments);
        }
        catch (TargetInvocationException e)
        {
            throw e.InnerException ?? e;
        }
    }

    private static object? ConvertToFrozenSet(
        Type elementType,
        string baseName,
        int length,
        Dictionary<string, int> headerIndexMap,
        string[] values,
        string? nullString,
        string? dateTimeFormat = null,
        string? timeSpanFormat = null)
    {
        var helperMethod = CsvTypeCache.GetFrozenSetHelperMethod(elementType, nameof(ConvertToFrozenSetHelper));
        return InvokeHelper(helperMethod, [baseName, length, headerIndexMap, values, nullString, dateTimeFormat, timeSpanFormat]);
    }

    internal static FrozenSet<T> ConvertToFrozenSetHelper<T>(
        string baseName,
        int length,
        Dictionary<string, int> headerIndexMap,
        string[] values,
        string? nullString,
        string? dateTimeFormat,
        string? timeSpanFormat)
    {
        var list = new List<T>(length);
        var elementType = typeof(T);
        var isPrimitive = IsPrimitiveOrSimpleType(elementType);

        for (var i = 0; i < length; i++)
        {
            var headerName = $"{baseName}[{i}]";
            object? convertedValue;

            if (isPrimitive)
            {
                if (!headerIndexMap.TryGetValue(headerName, out var index))
                {
                    throw new InvalidOperationException(string.Format(
                        CultureInfo.CurrentCulture,
                        Messages.Composite.CsvHeaderNotFound,
                        headerName));
                }

                convertedValue = ConvertStringValue(
                    elementType,
                    values[index],
                    nullString,
                    dateTimeFormat: dateTimeFormat,
                    timeSpanFormat: timeSpanFormat);
            }
            else
            {
                convertedValue = CreateRecordInstance(elementType, headerName, headerIndexMap, values, nullString);
            }

            list.Add((T)convertedValue!);
        }

        return list.ToFrozenSet();
    }

    private static object? ConvertToSingleColumnFrozenSet(
        Type elementType,
        string baseName,
        string separator,
        Dictionary<string, int> headerIndexMap,
        string[] values,
        string? nullString,
        CountRangeAttribute? countRange,
        string? dateTimeFormat = null,
        string? timeSpanFormat = null)
    {
        var helperMethod = CsvTypeCache.GetFrozenSetHelperMethod(elementType, nameof(ConvertToSingleColumnFrozenSetHelper));
        return InvokeHelper(helperMethod, [baseName, separator, headerIndexMap, values, nullString, countRange, dateTimeFormat, timeSpanFormat]);
    }

    internal static FrozenSet<T> ConvertToSingleColumnFrozenSetHelper<T>(
        string baseName,
        string separator,
        Dictionary<string, int> headerIndexMap,
        string[] values,
        string? nullString,
        CountRangeAttribute? countRange,
        string? dateTimeFormat,
        string? timeSpanFormat)
    {
        var parts = SplitSingleColumnCell(baseName, separator, headerIndexMap, values);

        var elementType = typeof(T);
        var set = new HashSet<T>(parts.Length);

        foreach (var part in parts)
        {
            var trimmedValue = part.Trim();
            var convertedValue = ConvertStringValue(
                elementType,
                trimmedValue,
                nullString,
                dateTimeFormat: dateTimeFormat,
                timeSpanFormat: timeSpanFormat);

            if (!set.Add((T)convertedValue!))
            {
                throw new ArgumentException(string.Format(
                    CultureInfo.CurrentCulture,
                    Messages.Composite.DuplicateValueInSet,
                    baseName,
                    convertedValue));
            }
        }

        ValidateCountRange(baseName, set.Count, countRange);
        return set.ToFrozenSet();
    }

    private static object? ConvertToFrozenDictionary(
        Type keyType,
        Type valueType,
        string baseName,
        int length,
        Dictionary<string, int> headerIndexMap,
        string[] values,
        string? nullString)
    {
        var helperMethod = CsvTypeCache.GetFrozenDictionaryHelperMethod(
            keyType,
            valueType,
            nameof(ConvertToFrozenDictionaryHelper));
        return InvokeHelper(helperMethod, [baseName, length, headerIndexMap, values, nullString]);
    }

    internal static FrozenDictionary<TKey, TValue> ConvertToFrozenDictionaryHelper<TKey, TValue>(
        string baseName,
        int length,
        Dictionary<string, int> headerIndexMap,
        string[] values,
        string? nullString)
        where TKey : notnull
    {
        var dictionary = new Dictionary<TKey, TValue>(length);
        var valueType = typeof(TValue);

        for (var i = 0; i < length; i++)
        {
            var elementBaseName = $"{baseName}[{i}]";
            var valueInstance = CreateRecordInstance(valueType, elementBaseName, headerIndexMap, values, nullString);

            var keyProperty = CsvTypeCache.GetKeyProperty(valueType);
            var keyInstance = keyProperty.GetValue(valueInstance);
            var key = (TKey)keyInstance!;

            if (!dictionary.TryAdd(key, (TValue)valueInstance!))
            {
                throw new ArgumentException(string.Format(
                    CultureInfo.CurrentCulture,
                    Messages.Composite.DuplicateKey,
                    key,
                    baseName));
            }
        }

        return dictionary.ToFrozenDictionary();
    }

    private static object CreateRecordInstance(
        Type recordType,
        string baseName,
        Dictionary<string, int> headerIndexMap,
        string[] values,
        string? nullString)
    {
        if (IsPrimitiveOrSimpleType(recordType))
        {
            if (!headerIndexMap.TryGetValue(baseName, out var index))
            {
                throw new InvalidOperationException(string.Format(
                    CultureInfo.CurrentCulture,
                    Messages.Composite.CsvHeaderNotFound,
                    baseName));
            }

            return ConvertStringValue(recordType, values[index], nullString);
        }

        var typeInfo = CsvTypeCache.GetTypeInfo(recordType);

        // for single parameter record types, map directly
        if (typeInfo.Parameters.Length == 1 &&
            IsPrimitiveOrSimpleType(typeInfo.Parameters[0].ParameterType))
        {
            if (headerIndexMap.TryGetValue(baseName, out var index))
            {
                var paramInfo = typeInfo.Parameters[0];
                var value = ConvertStringValue(
                    paramInfo.ParameterType,
                    values[index],
                    paramInfo.NullString ?? nullString,
                    paramInfo.IsKey,
                    paramInfo.DateTimeFormat,
                    paramInfo.TimeSpanFormat);

                ValidateScalar(paramInfo, value);
                return typeInfo.Constructor.Invoke([value]);
            }
        }

        var args = new object?[typeInfo.Parameters.Length];

        for (var i = 0; i < typeInfo.Parameters.Length; i++)
        {
            var paramInfo = typeInfo.Parameters[i];
            var fullName = $"{baseName}.{paramInfo.ColumnName}";
            args[i] = ConvertParameterValue(paramInfo, fullName, headerIndexMap, values, nullString);
        }

        return typeInfo.Constructor.Invoke(args);
    }

    private static object? ConvertParameterValue(
        ParameterMappingInfo paramInfo,
        string baseName,
        Dictionary<string, int> headerIndexMap,
        string[] values,
        string? nullString)
    {
        var effectiveNullString = paramInfo.NullString ?? nullString;

        if (paramInfo.CollectionKind != CollectionKind.None)
        {
            return paramInfo.CollectionKind switch
            {
                CollectionKind.ImmutableArray => ConvertToImmutableArray(
                    paramInfo.ElementType!,
                    baseName,
                    paramInfo.Length!.Value,
                    headerIndexMap,
                    values,
                    effectiveNullString,
                    paramInfo.DateTimeFormat,
                    paramInfo.TimeSpanFormat),
                CollectionKind.FrozenSet => ConvertToFrozenSet(
                    paramInfo.ElementType!,
                    baseName,
                    paramInfo.Length!.Value,
                    headerIndexMap,
                    values,
                    effectiveNullString,
                    paramInfo.DateTimeFormat,
                    paramInfo.TimeSpanFormat),
                CollectionKind.FrozenDictionary => ConvertToFrozenDictionary(
                    paramInfo.KeyType!,
                    paramInfo.ElementType!,
                    baseName,
                    paramInfo.Length!.Value,
                    headerIndexMap,
                    values,
                    effectiveNullString),
                _ => throw new InvalidOperationException(string.Format(
                    CultureInfo.CurrentCulture,
                    Messages.Composite.UnknownCollectionType,
                    paramInfo.CollectionKind)),
            };
        }

        if (IsPrimitiveOrSimpleType(paramInfo.ParameterType))
        {
            if (!headerIndexMap.TryGetValue(baseName, out var index))
            {
                throw new InvalidOperationException(string.Format(
                    CultureInfo.CurrentCulture,
                    Messages.Composite.CsvHeaderNotFound,
                    baseName));
            }

            var convertedValue = ConvertStringValue(
                paramInfo.ParameterType,
                values[index],
                effectiveNullString,
                paramInfo.IsKey,
                paramInfo.DateTimeFormat,
                paramInfo.TimeSpanFormat);

            ValidateScalar(paramInfo, convertedValue);
            return convertedValue;
        }

        return CreateRecordInstance(paramInfo.ParameterType, baseName, headerIndexMap, values, effectiveNullString);
    }

    private static bool IsPrimitiveOrSimpleType(Type type)
    {
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

        return underlyingType.IsPrimitive ||
               underlyingType.IsEnum ||
               underlyingType == typeof(string) ||
               underlyingType == typeof(decimal) ||
               underlyingType == typeof(DateTime) ||
               underlyingType == typeof(DateTimeOffset) ||
               underlyingType == typeof(TimeSpan) ||
               underlyingType == typeof(Guid);
    }
}
