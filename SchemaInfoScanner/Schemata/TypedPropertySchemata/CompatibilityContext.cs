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

    public string Current => Arguments[StartIndex];

    public string Consume()
    {
        if (StartIndex >= Arguments.Count)
        {
            throw new InvalidOperationException($"StartIndex {StartIndex} is out of range for the provided arguments.");
        }

        return Arguments[StartIndex++];
    }

    public void Collect(object value)
    {
        collectedValues.Add(value);
    }

    public void CollectNull()
    {
        StartIndex++;
        collectedValues.Add(null);
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
