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

    public static bool IsContainerType(INamedTypeSymbol typeSymbol)
    {
        foreach (var iface in ContainerInterfaces)
        {
            return typeSymbol.AllInterfaces.Any(i => i.ToDisplayString() == iface);
        }

        // Check if the type is in the relevant namespaces
        if (typeSymbol.ContainingNamespace != null)
        {
            var namespaceName = typeSymbol.ContainingNamespace.ToDisplayString();
            return ContainerNamespaces.Contains(namespaceName);
        }

        return false;
    }

    private static readonly string[] ContainerInterfaces = new[]
    {
        "System.Collections.IEnumerable",
        "System.Collections.ICollection",
        "System.Collections.IList",
        "System.Collections.IDictionary",
        "System.Collections.Generic.IEnumerable`1",
        "System.Collections.Generic.ICollection`1",
        "System.Collections.Generic.IList`1",
        "System.Collections.Generic.IDictionary`2",
        "System.Collections.Generic.ISet`1",
        "System.Collections.Generic.IReadOnlyCollection`1",
        "System.Collections.Generic.IReadOnlyList`1",
        "System.Collections.Generic.IReadOnlyDictionary`2",
        "System.Collections.Concurrent.IProducerConsumerCollection`1",
        "System.Collections.Concurrent.ConcurrentBag`1",
        "System.Collections.Concurrent.ConcurrentQueue`1",
        "System.Collections.Concurrent.ConcurrentStack`1",
        "System.Collections.Concurrent.ConcurrentDictionary`2",
        "System.Collections.Immutable.IImmutableList`1",
        "System.Collections.Immutable.IImmutableQueue`1",
        "System.Collections.Immutable.IImmutableStack`1",
        "System.Collections.Immutable.IImmutableSet`1",
        "System.Collections.Immutable.IImmutableDictionary`2"
    };

    private static readonly string[] ContainerNamespaces = new[]
    {
        "System.Collections",
        "System.Collections.Generic",
        "System.Collections.Concurrent",
        "System.Collections.Immutable"
    };
}
