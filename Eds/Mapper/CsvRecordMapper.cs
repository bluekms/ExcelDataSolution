using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Globalization;
using System.Reflection;
using Eds.Attributes;

namespace Eds.Mapper;

public static class CsvRecordMapper
{
    public static TRecord MapToRecord<TRecord>(string[] headers, string[] values)
        where TRecord : class
    {
        var type = typeof(TRecord);
        var ctor = type.GetConstructors().First();
        var parameters = ctor.GetParameters();

        var headerIndexMap = new Dictionary<string, int>();
        for (var i = 0; i < headers.Length; i++)
        {
            headerIndexMap[headers[i]] = i;
        }

        var args = new object?[parameters.Length];

        for (var i = 0; i < parameters.Length; i++)
        {
            var param = parameters[i];
            var columnNameAttr = param.GetCustomAttribute<ColumnNameAttribute>();
            var baseName = columnNameAttr?.Name ?? param.Name ?? string.Empty;

            args[i] = ConvertValue(param, baseName, headerIndexMap, values);
        }

        return (TRecord)ctor.Invoke(args);
    }

    private static object? ConvertValue(
        ParameterInfo param,
        string baseName,
        Dictionary<string, int> headerIndexMap,
        string[] values)
    {
        var paramType = param.ParameterType;
        var lengthAttr = param.GetCustomAttribute<LengthAttribute>();
        var nullStringAttr = param.GetCustomAttribute<NullStringAttribute>();

        if (lengthAttr is not null && paramType.IsGenericType)
        {
            var genericTypeDef = paramType.GetGenericTypeDefinition();
            if (genericTypeDef == typeof(ImmutableArray<>))
            {
                return ConvertToImmutableArray(
                    paramType,
                    baseName,
                    lengthAttr.Length,
                    headerIndexMap,
                    values,
                    nullStringAttr?.NullString);
            }

            if (genericTypeDef == typeof(FrozenSet<>))
            {
                return ConvertToFrozenSet(
                    paramType,
                    baseName,
                    lengthAttr.Length,
                    headerIndexMap,
                    values,
                    nullStringAttr?.NullString);
            }

            if (genericTypeDef == typeof(FrozenDictionary<,>))
            {
                return ConvertToFrozenDictionary(
                    paramType,
                    baseName,
                    lengthAttr.Length,
                    headerIndexMap,
                    values,
                    nullStringAttr?.NullString);
            }
        }

        if (!IsPrimitiveOrSimpleType(paramType))
        {
            return CreateRecordInstance(paramType, baseName, headerIndexMap, values, nullStringAttr?.NullString);
        }

        if (!headerIndexMap.TryGetValue(baseName, out var index))
        {
            throw new InvalidOperationException($"Header '{baseName}' not found in CSV");
        }

        return ConvertStringValue(paramType, values[index], nullStringAttr?.NullString);
    }

    private static object ConvertStringValue(Type targetType, string value, string? nullString)
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
            if (!Enum.IsDefined(targetType, parsed))
            {
                throw new ArgumentException($"'{value}' is not a defined value of enum '{targetType.Name}'");
            }

            return parsed;
        }

        return Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
    }

    private static object? ConvertToImmutableArray(
        Type paramType,
        string baseName,
        int length,
        Dictionary<string, int> headerIndexMap,
        string[] values,
        string? nullString)
    {
        var elementType = paramType.GetGenericArguments()[0];
        var array = Array.CreateInstance(elementType, length);

        for (var i = 0; i < length; i++)
        {
            var headerName = $"{baseName}[{i}]";
            object? convertedValue;

            if (IsPrimitiveOrSimpleType(elementType))
            {
                if (!headerIndexMap.TryGetValue(headerName, out var index))
                {
                    throw new InvalidOperationException($"Header '{headerName}' not found in CSV");
                }

                convertedValue = ConvertStringValue(elementType, values[index], nullString);
            }
            else
            {
                convertedValue = CreateRecordInstance(elementType, headerName, headerIndexMap, values, nullString);
            }

            array.SetValue(convertedValue, i);
        }

        var createMethod = typeof(ImmutableArray)
            .GetMethods()
            .Where(x => x.Name == nameof(ImmutableArray.Create))
            .Where(x => x.GetParameters().Length is 1)
            .First(x => x.GetParameters()[0].ParameterType.IsArray)
            .MakeGenericMethod(elementType);

        return createMethod.Invoke(null, [array]);
    }

    private static object? ConvertToFrozenSet(
        Type paramType,
        string baseName,
        int length,
        Dictionary<string, int> headerIndexMap,
        string[] values,
        string? nullString)
    {
        var elementType = paramType.GetGenericArguments()[0];

        var helperMethod = typeof(CsvRecordMapper)
            .GetMethod(nameof(ConvertToFrozenSetHelper), BindingFlags.NonPublic | BindingFlags.Static)!
            .MakeGenericMethod(elementType);

        return helperMethod.Invoke(null, [baseName, length, headerIndexMap, values, nullString]);
    }

    private static FrozenSet<T> ConvertToFrozenSetHelper<T>(
        string baseName,
        int length,
        Dictionary<string, int> headerIndexMap,
        string[] values,
        string? nullString)
    {
        var list = new List<T>();
        var elementType = typeof(T);

        for (var i = 0; i < length; i++)
        {
            var headerName = $"{baseName}[{i}]";
            object? convertedValue;

            if (IsPrimitiveOrSimpleType(elementType))
            {
                if (!headerIndexMap.TryGetValue(headerName, out var index))
                {
                    throw new InvalidOperationException($"Header '{headerName}' not found in CSV");
                }

                convertedValue = ConvertStringValue(elementType, values[index], nullString);
            }
            else
            {
                convertedValue = CreateRecordInstance(elementType, headerName, headerIndexMap, values, nullString);
            }

            list.Add((T)convertedValue!);
        }

        return list.ToFrozenSet();
    }

    private static object? ConvertToFrozenDictionary(
        Type paramType,
        string baseName,
        int length,
        Dictionary<string, int> headerIndexMap,
        string[] values,
        string? nullString)
    {
        var genericArgs = paramType.GetGenericArguments();
        var keyType = genericArgs[0];
        var valueType = genericArgs[1];

        var helperMethod = typeof(CsvRecordMapper)
            .GetMethod(nameof(ConvertToFrozenDictionaryHelper), BindingFlags.NonPublic | BindingFlags.Static)!
            .MakeGenericMethod(keyType, valueType);

        return helperMethod.Invoke(null, [baseName, length, headerIndexMap, values, nullString]);
    }

    private static FrozenDictionary<TKey, TValue> ConvertToFrozenDictionaryHelper<TKey, TValue>(
        string baseName,
        int length,
        Dictionary<string, int> headerIndexMap,
        string[] values,
        string? nullString)
        where TKey : notnull
    {
        var dictionary = new Dictionary<TKey, TValue>();
        var valueType = typeof(TValue);

        for (var i = 0; i < length; i++)
        {
            var elementBaseName = $"{baseName}[{i}]";
            var valueInstance = CreateRecordInstance(valueType, elementBaseName, headerIndexMap, values, nullString);

            var keyInstance = ExtractKeyFromValue(valueType, valueInstance!);
            dictionary.Add((TKey)keyInstance!, (TValue)valueInstance!);
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
                throw new InvalidOperationException($"Header '{baseName}' not found in CSV");
            }

            return ConvertStringValue(recordType, values[index], nullString);
        }

        var ctor = recordType.GetConstructors().First();
        var parameters = ctor.GetParameters();
        var args = new object?[parameters.Length];

        for (var i = 0; i < parameters.Length; i++)
        {
            var param = parameters[i];
            var columnNameAttr = param.GetCustomAttribute<ColumnNameAttribute>();
            var paramName = columnNameAttr?.Name ?? param.Name ?? string.Empty;
            var fullName = $"{baseName}.{paramName}";

            args[i] = ConvertParameterValue(param, fullName, headerIndexMap, values, nullString);
        }

        return ctor.Invoke(args);
    }

    private static object? ConvertParameterValue(
        ParameterInfo param,
        string baseName,
        Dictionary<string, int> headerIndexMap,
        string[] values,
        string? nullString)
    {
        var paramType = param.ParameterType;
        var lengthAttr = param.GetCustomAttribute<LengthAttribute>();
        var paramNullStringAttr = param.GetCustomAttribute<NullStringAttribute>();
        var effectiveNullString = paramNullStringAttr?.NullString ?? nullString;

        if (lengthAttr is not null && paramType.IsGenericType)
        {
            var genericTypeDef = paramType.GetGenericTypeDefinition();
            if (genericTypeDef == typeof(ImmutableArray<>))
            {
                return ConvertToImmutableArrayNested(
                    paramType,
                    baseName,
                    lengthAttr.Length,
                    headerIndexMap,
                    values,
                    effectiveNullString);
            }

            if (genericTypeDef == typeof(FrozenSet<>))
            {
                return ConvertToFrozenSetNested(
                    paramType,
                    baseName,
                    lengthAttr.Length,
                    headerIndexMap,
                    values,
                    effectiveNullString);
            }

            if (genericTypeDef == typeof(FrozenDictionary<,>))
            {
                return ConvertToFrozenDictionary(
                    paramType,
                    baseName,
                    lengthAttr.Length,
                    headerIndexMap,
                    values,
                    effectiveNullString);
            }
        }

        if (IsPrimitiveOrSimpleType(paramType))
        {
            if (!headerIndexMap.TryGetValue(baseName, out var index))
            {
                throw new InvalidOperationException($"Header '{baseName}' not found in CSV");
            }

            return ConvertStringValue(paramType, values[index], effectiveNullString);
        }

        return CreateRecordInstance(paramType, baseName, headerIndexMap, values, effectiveNullString);
    }

    private static object? ConvertToImmutableArrayNested(
        Type paramType,
        string baseName,
        int length,
        Dictionary<string, int> headerIndexMap,
        string[] values,
        string? nullString)
    {
        var elementType = paramType.GetGenericArguments()[0];
        var array = Array.CreateInstance(elementType, length);

        for (var i = 0; i < length; i++)
        {
            var headerName = $"{baseName}[{i}]";
            object? convertedValue;

            if (IsPrimitiveOrSimpleType(elementType))
            {
                if (!headerIndexMap.TryGetValue(headerName, out var index))
                {
                    throw new InvalidOperationException($"Header '{headerName}' not found in CSV");
                }

                convertedValue = ConvertStringValue(elementType, values[index], nullString);
            }
            else
            {
                convertedValue = CreateRecordInstance(elementType, headerName, headerIndexMap, values, nullString);
            }

            array.SetValue(convertedValue, i);
        }

        var createMethod = typeof(ImmutableArray)
            .GetMethods()
            .Where(x => x.Name == nameof(ImmutableArray.Create))
            .Where(x => x.GetParameters().Length is 1)
            .First(x => x.GetParameters()[0].ParameterType.IsArray)
            .MakeGenericMethod(elementType);

        return createMethod.Invoke(null, [array]);
    }

    private static object? ConvertToFrozenSetNested(
        Type paramType,
        string baseName,
        int length,
        Dictionary<string, int> headerIndexMap,
        string[] values,
        string? nullString)
    {
        var elementType = paramType.GetGenericArguments()[0];

        var helperMethod = typeof(CsvRecordMapper)
            .GetMethod(nameof(ConvertToFrozenSetHelper), BindingFlags.NonPublic | BindingFlags.Static)!
            .MakeGenericMethod(elementType);

        return helperMethod.Invoke(null, [baseName, length, headerIndexMap, values, nullString]);
    }

    private static object? ExtractKeyFromValue(Type valueType, object valueInstance)
    {
        var ctor = valueType.GetConstructors().First();
        var keyParam = ctor.GetParameters()
            .FirstOrDefault(p => p.GetCustomAttribute<KeyAttribute>() is not null);

        if (keyParam is null)
        {
            throw new InvalidOperationException(
                $"Value type '{valueType.Name}' must have a parameter with [Key] attribute");
        }

        var keyPropertyName = keyParam.Name!;
        var keyProperty = valueType.GetProperty(
            keyPropertyName,
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

        if (keyProperty is null)
        {
            throw new InvalidOperationException(
                $"Could not find property '{keyPropertyName}' on type '{valueType.Name}'");
        }

        return keyProperty.GetValue(valueInstance);
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
