using SchemaInfoScanner.Catalogs;

namespace SchemaInfoScanner.Schemata.TypedPropertySchemata;

public sealed record CompatibilityContext(
    EnumMemberCatalog EnumMemberCatalog,
    IReadOnlyList<string> Arguments,
    int StartIndex = 0,
    int? CollectionLength = null)
{
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

    public CompatibilityContext WithStartIndex(int newStartIndex)
    {
        if (newStartIndex < 0 || newStartIndex >= Arguments.Count)
        {
            throw new ArgumentOutOfRangeException(
                nameof(newStartIndex),
                "Start index is out of range for the provided arguments.",
                null);
        }

        return this with { StartIndex = newStartIndex };
    }

    public void Collect(object value)
    {
        collectedValues.Add(value);
    }

    public IReadOnlyList<object> GetCollectedValues()
    {
        return collectedValues;
    }

    private readonly List<object> collectedValues = new();
}
