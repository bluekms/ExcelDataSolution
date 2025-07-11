using SchemaInfoScanner.Catalogs;

namespace SchemaInfoScanner.Schemata.TypedPropertySchemata;

public sealed class CompatibilityContext
{
    public EnumMemberCatalog EnumMemberCatalog { get; }
    public IReadOnlyList<string> Arguments { get; }
    public int StartIndex { get; private set; }
    public int? CollectionLength { get; }

    public CompatibilityContext(
        EnumMemberCatalog enumMemberCatalog,
        IReadOnlyList<string> arguments,
        int startIndex = 0,
        int? collectionLength = null)
    {
        EnumMemberCatalog = enumMemberCatalog;
        Arguments = arguments;
        StartIndex = startIndex;
        CollectionLength = collectionLength;
    }

    public bool IsCollection => CollectionLength.HasValue;

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

    public void Collect(object? value)
    {
        ++StartIndex;
        collectedValues.Add(value);
    }

    public IReadOnlyList<object?> GetCollectedValues()
    {
        return collectedValues;
    }

    public override string ToString()
    {
        return $"CompatibilityContext[{string.Join(", ", Arguments)}]";
    }

    private readonly List<object?> collectedValues = new();
}
