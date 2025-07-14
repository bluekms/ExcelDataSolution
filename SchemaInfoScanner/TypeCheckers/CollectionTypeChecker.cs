using Microsoft.CodeAnalysis;

namespace SchemaInfoScanner.TypeCheckers;

public static class CollectionTypeChecker
{
    public static bool IsSupportedCollectionType(INamedTypeSymbol symbol)
    {
        return SetTypeChecker.IsSupportedSetType(symbol) ||
               ArrayTypeChecker.IsSupportedArrayType(symbol) ||
               MapTypeChecker.IsSupportedMapType(symbol);
    }

    public static bool IsPrimitiveCollection(INamedTypeSymbol symbol)
    {
        if (!ArrayTypeChecker.IsSupportedArrayType(symbol) &&
            !SetTypeChecker.IsSupportedSetType(symbol))
        {
            return false;
        }

        var typeArgument = (INamedTypeSymbol)symbol.TypeArguments.Single();

        return PrimitiveTypeChecker.IsSupportedPrimitiveType(typeArgument);
    }
}
