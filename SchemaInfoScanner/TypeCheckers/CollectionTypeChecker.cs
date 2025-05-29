using Microsoft.CodeAnalysis;

namespace SchemaInfoScanner.TypeCheckers;

public static class CollectionTypeChecker
{
    public static bool IsSupportedCollectionType(INamedTypeSymbol symbol)
    {
        return HashSetTypeChecker.IsSupportedHashSetType(symbol) ||
               ListTypeChecker.IsSupportedListType(symbol) ||
               DictionaryTypeChecker.IsSupportedDictionaryType(symbol);
    }

    public static bool IsPrimitiveCollection(INamedTypeSymbol symbol)
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
