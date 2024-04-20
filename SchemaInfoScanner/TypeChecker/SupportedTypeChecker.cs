using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SchemaInfoScanner.TypeChecker;

public static class SupportedTypeChecker
{
    public static void Check(INamedTypeSymbol symbol, SemanticModel semanticModel, IReadOnlyList<RecordDeclarationSyntax> recordDeclarationList)
    {
        if (PrimitiveTypeChecker.IsSupportedPrimitiveType(symbol))
        {
            return;
        }

        if (IsContainerType(symbol))
        {
            CheckSupportedContainerType(symbol, semanticModel, recordDeclarationList);
        }
        else
        {
            RecordTypeChecker.Check(symbol, semanticModel, recordDeclarationList);
        }
    }

    private static void CheckSupportedContainerType(INamedTypeSymbol symbol, SemanticModel semanticModel, IReadOnlyList<RecordDeclarationSyntax> recordDeclarationList)
    {
        if (HashSetTypeChecker.IsSupportedHashSetType(symbol))
        {
            HashSetTypeChecker.Check(symbol);
        }
        else if (ListTypeChecker.IsSupportedListType(symbol))
        {
            ListTypeChecker.Check(symbol, semanticModel, recordDeclarationList);
        }
        else if (DictionaryTypeChecker.IsSupportedDictionaryType(symbol))
        {
            DictionaryTypeChecker.Check(symbol, semanticModel, recordDeclarationList);
        }
        else
        {
            throw new NotSupportedException($"{symbol} is not supported container type.");
        }
    }

    private static bool IsContainerType(INamedTypeSymbol symbol)
    {
        return HashSetTypeChecker.IsSupportedHashSetType(symbol) ||
               ListTypeChecker.IsSupportedListType(symbol) ||
               DictionaryTypeChecker.IsSupportedDictionaryType(symbol);
    }
}
