using Microsoft.CodeAnalysis;
using SchemaInfoScanner.Schemata;

namespace SchemaInfoScanner.TypeCheckers;

public static class ContainerTypeChecker
{
    public static bool IsSupportedContainerType(INamedTypeSymbol symbol)
    {
        return HashSetTypeChecker.IsSupportedHashSetType(symbol) ||
               ListTypeChecker.IsSupportedListType(symbol) ||
               DictionaryTypeChecker.IsSupportedDictionaryType(symbol);
    }

    public static bool IsSupportedContainerType(RecordParameterSchema recordParameter)
    {
        return HashSetTypeChecker.IsSupportedHashSetType(recordParameter) ||
               ListTypeChecker.IsSupportedListType(recordParameter) ||
               DictionaryTypeChecker.IsSupportedDictionaryType(recordParameter);
    }
}
