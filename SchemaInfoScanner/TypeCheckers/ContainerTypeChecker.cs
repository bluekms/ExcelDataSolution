using Microsoft.CodeAnalysis;

namespace SchemaInfoScanner.TypeCheckers;

public static class ContainerTypeChecker
{
    public static bool IsSupportedContainerType(INamedTypeSymbol symbol)
    {
        return HashSetTypeChecker.IsSupportedHashSetType(symbol) ||
               ListTypeChecker.IsSupportedListType(symbol) ||
               DictionaryTypeChecker.IsSupportedDictionaryType(symbol);
    }
}
