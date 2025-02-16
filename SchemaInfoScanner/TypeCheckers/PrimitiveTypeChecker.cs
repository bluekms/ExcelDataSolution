using Microsoft.CodeAnalysis;
using SchemaInfoScanner.Exceptions;
using SchemaInfoScanner.Extensions;
using SchemaInfoScanner.Schemata;
using StaticDataAttribute;

namespace SchemaInfoScanner.TypeCheckers;

internal static class PrimitiveTypeChecker
{
    public static void Check(RawParameterSchema rawParameter)
    {
        if (!IsSupportedPrimitiveType(rawParameter.NamedTypeSymbol))
        {
            throw new TypeNotSupportedException($"{rawParameter.ParameterName.FullName} is not supported primitive type.");
        }

        if (!rawParameter.IsNullable())
        {
            if (rawParameter.HasAttribute<NullStringAttribute>())
            {
                throw new InvalidUsageException($"{rawParameter.ParameterName.FullName} is not nullable, so you can't use {nameof(NullStringAttribute)}.");
            }
        }

        CheckUnavailableAttribute(rawParameter);
        CheckRequiredAttribute(rawParameter);
    }

    public static bool IsSupportedPrimitiveType(INamedTypeSymbol symbol)
    {
        var isNullable = symbol.OriginalDefinition.SpecialType is SpecialType.System_Nullable_T;
        var underlyingType = isNullable
            ? symbol.TypeArguments[0]
            : symbol;

        if (CheckSpecialType(underlyingType))
        {
            return true;
        }

        if (CheckEnumType(underlyingType))
        {
            return true;
        }

        if (CheckDateTimeType(underlyingType))
        {
            return true;
        }

        if (CheckTimeSpanType(underlyingType))
        {
            return true;
        }

        return false;
    }

    private static void CheckUnavailableAttribute(RawParameterSchema rawParameter)
    {
        if (!rawParameter.IsNullable() && rawParameter.HasAttribute<NullStringAttribute>())
        {
            throw new InvalidUsageException($"{rawParameter.ParameterName.FullName} is not nullable, so you can't use {nameof(NullStringAttribute)}.");
        }

        if (rawParameter.HasAttribute<MaxCountAttribute>())
        {
            throw new InvalidUsageException($"{nameof(MaxCountAttribute)} is not available for primitive type {rawParameter.ParameterName.FullName}.");
        }

        if (rawParameter.HasAttribute<SingleColumnContainerAttribute>())
        {
            throw new InvalidUsageException($"{nameof(SingleColumnContainerAttribute)} is not available for primitive type {rawParameter.ParameterName.FullName}.");
        }
    }

    private static void CheckRequiredAttribute(RawParameterSchema rawParameter)
    {
        var symbol = rawParameter.NamedTypeSymbol;
        var isNullable = symbol.OriginalDefinition.SpecialType is SpecialType.System_Nullable_T;
        var underlyingType = isNullable
            ? symbol.TypeArguments[0]
            : symbol;

        if (CheckDateTimeType(underlyingType))
        {
            if (!rawParameter.HasAttribute<DateTimeFormatAttribute>())
            {
                throw new AttributeNotFoundException<DateTimeFormatAttribute>(rawParameter.ParameterName.FullName);
            }
        }

        if (CheckTimeSpanType(underlyingType))
        {
            if (!rawParameter.HasAttribute<TimeSpanFormatAttribute>())
            {
                throw new AttributeNotFoundException<TimeSpanFormatAttribute>(rawParameter.ParameterName.FullName);
            }
        }
    }

    private static bool CheckSpecialType(ITypeSymbol symbol)
    {
        return symbol.SpecialType switch
        {
            SpecialType.System_Boolean => true,
            SpecialType.System_Char => true,
            SpecialType.System_SByte => true,
            SpecialType.System_Byte => true,
            SpecialType.System_Int16 => true,
            SpecialType.System_UInt16 => true,
            SpecialType.System_Int32 => true,
            SpecialType.System_UInt32 => true,
            SpecialType.System_Int64 => true,
            SpecialType.System_UInt64 => true,
            SpecialType.System_Single => true,
            SpecialType.System_Double => true,
            SpecialType.System_Decimal => true,
            SpecialType.System_String => true,
            _ => false
        };
    }

    private static bool CheckEnumType(ITypeSymbol symbol)
    {
        return symbol.TypeKind is TypeKind.Enum;
    }

    private static bool CheckDateTimeType(ITypeSymbol symbol)
    {
        return symbol.Name is "DateTime";
    }

    private static bool CheckTimeSpanType(ITypeSymbol symbol)
    {
        return symbol.Name is "TimeSpan";
    }
}
