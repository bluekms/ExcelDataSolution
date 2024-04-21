using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SchemaInfoScanner.TypeCheckers;

public static class ListTypeChecker
{
    public static bool IsSupportedListType(INamedTypeSymbol symbol)
    {
        return symbol.Name.StartsWith("List", StringComparison.Ordinal) &&
               symbol.TypeArguments is [INamedTypeSymbol];
    }

    public static void Check(INamedTypeSymbol symbol, SemanticModel semanticModel, IReadOnlyList<RecordDeclarationSyntax> recordDeclarationList)
    {
        if (!IsSupportedListType(symbol))
        {
            throw new NotSupportedException($"{symbol} is not supported list type.");
        }

        if (symbol.TypeArguments.First() is not INamedTypeSymbol namedTypeSymbol)
        {
            throw new NotSupportedException($"{symbol} is not INamedTypeSymbol for List<T>.");
        }

        if (!PrimitiveTypeChecker.IsSupportedPrimitiveType(namedTypeSymbol))
        {
            RecordTypeChecker.Check(namedTypeSymbol, semanticModel, recordDeclarationList);
        }
    }
}
