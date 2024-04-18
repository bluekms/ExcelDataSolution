using Microsoft.CodeAnalysis;

namespace SchemaInfoScanner.TypeChecker;

public class HashSetTypeChecker
{
    public static bool IsHashSet(INamedTypeSymbol symbol)
    {
        return symbol.Name.StartsWith("HashSet", StringComparison.Ordinal);
    }

    public static bool CheckSupportedType(INamedTypeSymbol symbol)
    {
        if (!IsHashSet(symbol) ||
            symbol.TypeArguments is not [INamedTypeSymbol namedTypeSymbol])
        {
            return false;
        }

        return PrimitiveTypeChecker.CheckSupportedType(namedTypeSymbol);
    }
}
