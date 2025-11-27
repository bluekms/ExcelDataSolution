using SchemaInfoScanner.Catalogs;

namespace SchemaInfoScanner.Schemata.TypedPropertySchemata;

public sealed class CompatibilityContext
{
    private enum CollectMode
    {
        None,
        All,
        KeyOnly,
    }

    private CompatibilityContext(
        EnumMemberCatalog enumMemberCatalog,
        IReadOnlyList<string> arguments,
        CollectMode collectMode,
        int startPosition = 0)
    {
        EnumMemberCatalog = enumMemberCatalog;
        Arguments = arguments;
        this.collectMode = collectMode;
        Position = startPosition;
    }

    public static CompatibilityContext CreateNoCollect(
        EnumMemberCatalog enumMemberCatalog,
        IReadOnlyList<string> arguments,
        int startPosition = 0)
        => new(enumMemberCatalog, arguments, CollectMode.None, startPosition);

    public static CompatibilityContext CreateCollectAll(
        EnumMemberCatalog enumMemberCatalog,
        IReadOnlyList<string> arguments,
        int startPosition = 0)
        => new(enumMemberCatalog, arguments, CollectMode.All, startPosition);

    public static CompatibilityContext CreateCollectKey(
        EnumMemberCatalog enumMemberCatalog,
        IReadOnlyList<string> arguments,
        int startPosition = 0)
        => new(enumMemberCatalog, arguments, CollectMode.KeyOnly, startPosition);

    public EnumMemberCatalog EnumMemberCatalog { get; }
    public IReadOnlyList<string> Arguments { get; }
    public int Position { get; private set; }
    private readonly CollectMode collectMode;
    private readonly List<object?> duplicateCandidates = new();
    private readonly List<object?> keyScopeComponents = new();
    private bool isKeyScope;

    public string Current => Arguments[Position];

    public string Consume()
    {
        if (Position >= Arguments.Count)
        {
            throw new InvalidOperationException($"StartIndex {Position} is out of range for the provided arguments.");
        }

        return Arguments[Position++];
    }

    public void Collect(object? key)
    {
        if (collectMode is CollectMode.None)
        {
            return;
        }

        if (collectMode is CollectMode.KeyOnly && isKeyScope)
        {
            keyScopeComponents.Add(key);
            return;
        }

        if (collectMode is CollectMode.All)
        {
            duplicateCandidates.Add(key);
            return;
        }

        throw new InvalidOperationException("Unreachable code reached in Collect method.");
    }

    public void ValidateNoDuplicates()
    {
        if (collectMode is CollectMode.None)
        {
            throw new InvalidOperationException($"Cannot validate duplicates when CollectMode is None: {this}");
        }

        if (duplicateCandidates.Count != duplicateCandidates.Distinct().Count())
        {
            throw new InvalidOperationException($"Duplicate values found in the argument: {this}");
        }
    }

    public void ConsumeNull()
    {
        Position++;
        Collect(null);
    }

    public void Skip(int count = 1)
    {
        Position += count;
    }

    public void BeginKeyScope()
    {
        if (collectMode is not CollectMode.KeyOnly)
        {
            throw new InvalidOperationException($"BeginKeyScope can only be called in KeyOnly collect mode: {this}");
        }

        if (isKeyScope)
        {
            throw new InvalidOperationException($"Nested key scopes are not allowed: {this}");
        }

        isKeyScope = true;
        keyScopeComponents.Clear();
    }

    public void EndKeyScope()
    {
        if (collectMode is not CollectMode.KeyOnly)
        {
            throw new InvalidOperationException($"EndKeyScope can only be called in KeyOnly collect mode: {this}");
        }

        if (!isKeyScope)
        {
            throw new InvalidOperationException($"EndKeyScope called without a matching BeginKeyScope: {this}");
        }

        if (keyScopeComponents.Count == 0)
        {
            throw new InvalidOperationException($"No components collected in key scope: {this}");
        }

        object? keyObject;

        if (keyScopeComponents.Count == 1)
        {
            keyObject = keyScopeComponents[0];
        }
        else
        {
            keyObject = new RecordKey(keyScopeComponents.ToArray());
        }

        duplicateCandidates.Add(keyObject);
        keyScopeComponents.Clear();
        isKeyScope = false;
    }

    public override string ToString()
    {
        return $"CompatibilityContext[{string.Join(", ", Arguments)}]";
    }

    private sealed class RecordKey(object?[] values)
    {
        private readonly object?[] values = values;

        public override bool Equals(object? obj)
        {
            if (obj is not RecordKey other)
            {
                return false;
            }

            if (values.Length != other.values.Length)
            {
                return false;
            }

            for (var i = 0; i < values.Length; i++)
            {
                if (!Equals(values[i], other.values[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            var hash = default(HashCode);

            for (var i = 0; i < values.Length; i++)
            {
                hash.Add(values[i]);
            }

            return hash.ToHashCode();
        }

        public override string ToString()
        {
            return $"({string.Join(", ", values.Select(v => v?.ToString() ?? "null"))})";
        }
    }
}
