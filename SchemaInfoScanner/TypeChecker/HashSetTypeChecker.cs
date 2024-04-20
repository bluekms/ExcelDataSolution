using Microsoft.CodeAnalysis;

namespace SchemaInfoScanner.TypeChecker;

public static class HashSetTypeChecker
{
    public static bool IsSupportedHashSetType(INamedTypeSymbol symbol)
    {
        return symbol.Name.StartsWith("HashSet", StringComparison.Ordinal) &&
               symbol.TypeArguments is [INamedTypeSymbol];
    }

    public static void Check(INamedTypeSymbol symbol)
    {
        if (!IsSupportedHashSetType(symbol))
        {
            throw new NotSupportedException($"{symbol} is not supported hash set type.");
        }

        if (symbol.TypeArguments.First() is not INamedTypeSymbol namedTypeSymbol)
        {
            throw new NotSupportedException($"{symbol} is not INamedTypeSymbol for HashSet<T>.");
        }

        try
        {
            PrimitiveTypeChecker.Check(namedTypeSymbol);
        }
        catch (Exception e)
        {
            throw new NotSupportedException($"Not support hash set with not primitive type.", e);
        }
    }
}
