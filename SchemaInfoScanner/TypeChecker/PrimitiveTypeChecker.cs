using Microsoft.CodeAnalysis;

namespace SchemaInfoScanner.TypeChecker;

public class PrimitiveTypeChecker
{
    public static bool IsSupportedPrimitiveType(INamedTypeSymbol symbol)
    {
        var specialTypeCheck = symbol.SpecialType switch
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

        return specialTypeCheck || symbol.TypeKind == TypeKind.Enum;
    }

    public static void Check(INamedTypeSymbol symbol)
    {
        if (!IsSupportedPrimitiveType(symbol))
        {
            throw new NotSupportedException($"{symbol} is not supported primitive type.");
        }
    }
}
