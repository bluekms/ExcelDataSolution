using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SchemaInfoScanner.TypeCheckers;

public static class RecordTypeChecker
{
    private static readonly string[] RecordMethodNames = { "Equals", "GetHashCode", "ToString", "PrintMembers" };

    public static bool IsSupportedRecordType(INamedTypeSymbol symbol)
    {
        var methodSymbols = symbol.GetMembers().OfType<IMethodSymbol>().Select(x => x.Name);
        return !RecordMethodNames.Except(methodSymbols).Any();
    }

    public static void Check(INamedTypeSymbol symbol, SemanticModel semanticModel, IReadOnlyList<RecordDeclarationSyntax> recordDeclarationList)
    {
        if (!IsSupportedRecordType(symbol))
        {
            throw new NotSupportedException($"{symbol} is not supported record type.");
        }

        foreach (var member in symbol.GetMembers()
                     .Where(x => !x.IsImplicitlyDeclared)
                     .Skip(1)
                     .OfType<IPropertySymbol>())
        {
            if (member.Type is not INamedTypeSymbol namedSymbol)
            {
                throw new NotSupportedException($"{symbol}.{member} is not INamedTypeSymbol for record type.");
            }

            SupportedTypeChecker.Check(namedSymbol, semanticModel, recordDeclarationList);
        }
    }
}
