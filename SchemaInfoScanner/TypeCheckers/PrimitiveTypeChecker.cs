using Microsoft.CodeAnalysis;
using SchemaInfoScanner.Exceptions;
using SchemaInfoScanner.Extensions;
using SchemaInfoScanner.Schemata;
using StaticDataAttribute;

namespace SchemaInfoScanner.TypeCheckers;

internal static class PrimitiveTypeChecker
{
    public static void Check(PropertySchemaBase property)
    {
        if (!IsSupportedPrimitiveType(property.NamedTypeSymbol))
        {
            throw new TypeNotSupportedException($"{property.PropertyName.FullName} is not supported primitive type.");
        }

        if (!property.IsNullable())
        {
            if (property.HasAttribute<NullStringAttribute>())
            {
                throw new InvalidUsageException($"{property.PropertyName.FullName} is not nullable, so you can't use {nameof(NullStringAttribute)}.");
            }
        }

        CheckUnavailableAttribute(property);
        CheckRequiredAttribute(property);
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

        return CheckTimeSpanType(underlyingType);
    }

    private static void CheckUnavailableAttribute(PropertySchemaBase property)
    {
        if (!property.IsNullable() && property.HasAttribute<NullStringAttribute>())
        {
            throw new InvalidUsageException($"{property.PropertyName.FullName} is not nullable, so you can't use {nameof(NullStringAttribute)}.");
        }

        if (property.HasAttribute<MaxCountAttribute>())
        {
            throw new InvalidUsageException($"{nameof(MaxCountAttribute)} is not available for primitive type {property.PropertyName.FullName}.");
        }

        if (property.HasAttribute<SingleColumnContainerAttribute>())
        {
            throw new InvalidUsageException($"{nameof(SingleColumnContainerAttribute)} is not available for primitive type {property.PropertyName.FullName}.");
        }
    }

    private static void CheckRequiredAttribute(PropertySchemaBase property)
    {
        var symbol = property.NamedTypeSymbol;
        var isNullable = symbol.OriginalDefinition.SpecialType is SpecialType.System_Nullable_T;
        var underlyingType = isNullable
            ? symbol.TypeArguments[0]
            : symbol;

        if (CheckDateTimeType(underlyingType))
        {
            if (!property.HasAttribute<DateTimeFormatAttribute>())
            {
                throw new AttributeNotFoundException<DateTimeFormatAttribute>(property.PropertyName.FullName);
            }
        }

        if (CheckTimeSpanType(underlyingType))
        {
            if (!property.HasAttribute<TimeSpanFormatAttribute>())
            {
                throw new AttributeNotFoundException<TimeSpanFormatAttribute>(property.PropertyName.FullName);
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
