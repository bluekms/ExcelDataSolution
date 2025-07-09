using SchemaInfoScanner.Catalogs;

namespace SchemaInfoScanner.Schemata.CompatibilityContexts;

public sealed record CompatibilityContext(
    EnumMemberCatalog EnumMemberCatalog,
    IReadOnlyList<string> Arguments,
    int StartIndex = 0,
    int? CollectionLength = null) : ICompatibilityContext
{
    public bool IsCollection
    {
        get { return CollectionLength.HasValue; }
    }

    public ICompatibilityContext WithStartIndex(int newStartIndex)
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
}
