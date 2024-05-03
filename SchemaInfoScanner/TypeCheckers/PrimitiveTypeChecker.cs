using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using StaticDataAttribute;
using StaticDataAttribute.Extensions;

namespace SchemaInfoScanner.TypeCheckers;

public static class PrimitiveTypeChecker
{
    public static bool IsSupportedPrimitiveType(INamedTypeSymbol symbol)
    {
        var isNullable = symbol.OriginalDefinition.SpecialType is SpecialType.System_Nullable_T;

        var specialTypeCheck = isNullable
            ? CheckSpecialType(symbol.TypeArguments.First().SpecialType)
            : CheckSpecialType(symbol.SpecialType);

        var typeKindCheck = isNullable
            ? CheckEnumType(symbol.TypeArguments.First().TypeKind)
            : CheckEnumType(symbol.TypeKind);

        var isSupported = specialTypeCheck || typeKindCheck;
        if (!isSupported)
        {
            return false;
        }

        // TODO Check NullString Attribute
        return true;
    }

    public static void Check(INamedTypeSymbol symbol)
    {
        if (!IsSupportedPrimitiveType(symbol))
        {
            throw new NotSupportedException($"{symbol} is not supported primitive type.");
        }
    }

    private static bool CheckSpecialType(SpecialType specialType)
    {
        return specialType switch
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

    private static bool CheckEnumType(TypeKind typeKind)
    {
        return typeKind is TypeKind.Enum;
    }
}
