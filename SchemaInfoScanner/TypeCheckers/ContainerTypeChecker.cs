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
        if (typeSymbol.TypeArguments.Length == 0)
        {
            return false;
        }

        var name = typeSymbol.ConstructedFrom.ToDisplayString();
        var index = name.IndexOf('<');
        if (index == -1)
        {
            return false;
        }

        return ContainerNames.Contains(name.Substring(0, index + 1));
    }

    private static readonly string[] ContainerNames = new[]
    {
        "List<",
        "Dictionary<",
        "HashSet<",
        "LinkedList<",
        "Queue<",
        "Stack<",
        "SortedList<",
        "SortedDictionary<",
        "SortedSet<",
        "ICollection<",
        "IList<",
        "IDictionary<",
        "IEnumerable<",
        "IReadOnlyCollection<",
        "IReadOnlyList<",
        "IReadOnlyDictionary<",
        "ConcurrentDictionary<",
        "ConcurrentBag<",
        "ConcurrentQueue<",
        "ConcurrentStack<",
        "ImmutableList<",
        "ImmutableDictionary<",
        "ImmutableHashSet<",
        "ImmutableQueue<",
        "ImmutableStack<",
        "ImmutableSortedSet<",
        "ImmutableSortedDictionary<",
        "BlockingCollection<"
    };
}
