using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SchemaInfoScanner.TypeChecker;

public class SupportedTypeChecker
{
    public static bool CheckSupportedType(INamedTypeSymbol symbol, SemanticModel semanticModel, IReadOnlyList<RecordDeclarationSyntax> recordDeclarationList)
    {
        if (PrimitiveTypeChecker.CheckSupportedType(symbol))
        {
            return true;
        }

        if (IsContainerType(symbol))
        {
            return IsSupportedContainerType(symbol, semanticModel, recordDeclarationList);
        }

        return RecordTypeChecker.CheckSupportedType(symbol, semanticModel, recordDeclarationList);
    }

    private static bool IsSupportedContainerType(INamedTypeSymbol symbol, SemanticModel semanticModel, IReadOnlyList<RecordDeclarationSyntax> recordDeclarationList)
    {
        if (HashSetTypeChecker.IsHashSet(symbol))
        {
            return HashSetTypeChecker.CheckSupportedType(symbol);
        }
        else if (ListTypeChecker.IsList(symbol))
        {
            return ListTypeChecker.CheckSupportedType(symbol, semanticModel, recordDeclarationList);
        }
        else if (DictionaryTypeChecker.IsDictionary(symbol))
        {
            return DictionaryTypeChecker.CheckSupportedType(symbol, semanticModel, recordDeclarationList);
        }
        else
        {
            return false;
        }
    }

    private static bool IsContainerType(INamedTypeSymbol symbol)
    {
        return HashSetTypeChecker.IsHashSet(symbol) ||
               ListTypeChecker.IsList(symbol) ||
               DictionaryTypeChecker.IsDictionary(symbol);
    }
}
