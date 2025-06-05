using SchemaInfoScanner.Catalogs;

namespace SchemaInfoScanner.Schemata;

public sealed record CompatibilityContext
{
    public IReadOnlyList<string> Arguments { get; }
    public int StartIndex { get; set; }
    public EnumMemberCatalog EnumMemberCatalog { get; }
    public int? CollectionLength { get; }

    private CompatibilityContext(
        IReadOnlyList<string> arguments,
        int startIndex,
        EnumMemberCatalog enumMemberCatalog,
        int? collectionLength)
    {
        Arguments = arguments;
        StartIndex = startIndex;
        EnumMemberCatalog = enumMemberCatalog;
        CollectionLength = collectionLength;
    }

    public static CompatibilityContext CreateContext(
        IReadOnlyList<string> arguments,
        int startIndex,
        EnumMemberCatalog enumMemberCatalog)
    {
        if (startIndex >= arguments.Count)
        {
            throw new InvalidOperationException($"StartIndex {startIndex} is out of range for the provided arguments.");
        }

        return new(arguments, startIndex, enumMemberCatalog, null);
    }

    public static CompatibilityContext CreateCollectionContext(
        IReadOnlyList<string> arguments,
        int startIndex,
        EnumMemberCatalog enumMemberCatalog,
        int collectionLength)
    {
        if (startIndex >= arguments.Count)
        {
            throw new InvalidOperationException($"StartIndex {startIndex} is out of range for the provided arguments.");
        }

        return new(arguments, startIndex, enumMemberCatalog, collectionLength);
    }

    public static CompatibilityContext CreateSingleColumnCollectionContext(
        CompatibilityContext context,
        string separator)
    {
        var splitValues = context.CurrentArgument
            .Split(separator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return new(splitValues, 0, context.EnumMemberCatalog, splitValues.Length);
    }

    public string CurrentArgument
    {
        get
        {
            if (StartIndex >= Arguments.Count)
            {
                throw new InvalidOperationException($"StartIndex {StartIndex} is out of range for the provided arguments.");
            }

            return Arguments[StartIndex];
        }
    }

    public bool IsCollection => CollectionLength.HasValue;

    public override string ToString()
    {
        if (CollectionLength.HasValue)
        {
            var endIndex = StartIndex + CollectionLength.Value - 1;
            var values = Arguments
                .Skip(StartIndex)
                .Take(CollectionLength.Value)
                .Select(s => $"'{s}'");

            return $"Columns[{StartIndex}-{endIndex}] = [{string.Join(", ", values)}]";
        }
        else
        {
            var value = StartIndex < Arguments.Count ? Arguments[StartIndex] : "<out of range>";
            return $"Column[{StartIndex}] = '{value}'";
        }
    }
}
