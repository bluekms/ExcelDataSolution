using SchemaInfoScanner.Catalogs;

namespace SchemaInfoScanner.Schemata.TypedPropertySchemata;

public sealed class CompatibilityContext(
    EnumMemberCatalog enumMemberCatalog,
    IReadOnlyList<string> arguments,
    int startIndex = 0)
{
    public EnumMemberCatalog EnumMemberCatalog { get; } = enumMemberCatalog;
    public IReadOnlyList<string> Arguments { get; } = arguments;
    public int StartIndex { get; private set; } = startIndex;

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

    private readonly List<object?> collectedKeys = new();
    private readonly List<object?> collectedValues = new();
}
