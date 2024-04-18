using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SchemaInfoScanner.TypeChecker;

public static class RecordTypeChecker
{
    private static readonly string[] RecordMethodNames = { "Equals", "GetHashCode", "ToString", "PrintMembers" };

    public static bool IsRecordType(INamedTypeSymbol symbol)
    {
        var methodSymbols = symbol.GetMembers().OfType<IMethodSymbol>().Select(x => x.Name);
        return !RecordMethodNames.Except(methodSymbols).Any();
    }

    public static bool CheckSupportedType(INamedTypeSymbol symbol, SemanticModel semanticModel, IReadOnlyList<RecordDeclarationSyntax> recordDeclarationList)
    {
        if (!IsRecordType(symbol))
        {
            return false;
        }

        foreach (var member in symbol.GetMembers()
                     .Where(x => !x.IsImplicitlyDeclared)
                     .Skip(1)
                     .OfType<IPropertySymbol>())
        {
            var namedSymbol = member.Type as INamedTypeSymbol;
            if (!SupportedTypeChecker.CheckSupportedType(namedSymbol!, semanticModel, recordDeclarationList))
            {
                return false;
            }
        }

        return true;
    }
}
