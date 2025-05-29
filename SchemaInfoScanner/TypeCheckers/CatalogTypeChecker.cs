using Microsoft.CodeAnalysis;

namespace SchemaInfoScanner.TypeCheckers;

public static class CatalogTypeChecker
{
    public static bool IsSupportedCatalogType(INamedTypeSymbol symbol)
    {
        return HashSetTypeChecker.IsSupportedHashSetType(symbol) ||
               ListTypeChecker.IsSupportedListType(symbol) ||
               DictionaryTypeChecker.IsSupportedDictionaryType(symbol);
    }

    public static bool IsPrimitiveCatalog(INamedTypeSymbol symbol)
    {
        if (!ListTypeChecker.IsSupportedListType(symbol) &&
            !HashSetTypeChecker.IsSupportedHashSetType(symbol))
        {
            return false;
        }

        var typeArgument = (INamedTypeSymbol)symbol.TypeArguments.Single();

        return PrimitiveTypeChecker.IsSupportedPrimitiveType(typeArgument);
    }
}
