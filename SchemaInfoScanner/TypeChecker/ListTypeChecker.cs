using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SchemaInfoScanner.TypeChecker;

public class ListTypeChecker
{
    public static bool IsList(INamedTypeSymbol symbol)
    {
        return symbol.Name.StartsWith("List", StringComparison.Ordinal);
    }

    public static bool CheckSupportedType(INamedTypeSymbol symbol, SemanticModel semanticModel, IReadOnlyList<RecordDeclarationSyntax> recordDeclarationList)
    {
        if (!IsList(symbol) || symbol.TypeArguments is not [INamedTypeSymbol namedTypeSymbol])
        {
            return false;
        }

        if (PrimitiveTypeChecker.CheckSupportedType(namedTypeSymbol))
        {
            return true;
        }

        return RecordTypeChecker.CheckSupportedType(namedTypeSymbol, semanticModel, recordDeclarationList);
    }
}
