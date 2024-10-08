using Microsoft.CodeAnalysis;

namespace SchemaInfoScanner.TypeCheckers;

internal static class ContainerTypeChecker
{
    public static bool IsSupportedContainerType(INamedTypeSymbol symbol)
    {
        return HashSetTypeChecker.IsSupportedHashSetType(symbol) ||
               ListTypeChecker.IsSupportedListType(symbol) ||
               DictionaryTypeChecker.IsSupportedDictionaryType(symbol);
    }

    public static bool IsPrimitiveContainer(INamedTypeSymbol symbol)
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
