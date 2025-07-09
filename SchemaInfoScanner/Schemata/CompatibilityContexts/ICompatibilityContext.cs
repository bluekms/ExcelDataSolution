using SchemaInfoScanner.Catalogs;

namespace SchemaInfoScanner.Schemata.CompatibilityContexts;

public interface ICompatibilityContext
{
    IReadOnlyList<string> Arguments { get; }
    EnumMemberCatalog EnumMemberCatalog { get; }
    int StartIndex { get; }
    int? CollectionLength { get; }

    bool IsCollection { get; }

    string CurrentArgument
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

    ICompatibilityContext WithStartIndex(int newStartIndex);

    string ToString()
    {
        if (CollectionLength.HasValue)
        {
            var endIndex = StartIndex + CollectionLength.Value - 1;
            var values = Arguments
                .Skip(StartIndex)
                .Take(CollectionLength.Value)
                .Select(s => $"'{s}'");

            return $"{GetType().Name}: Columns[{StartIndex}-{endIndex}] = [{string.Join(", ", values)}]";
        }
        else
        {
            var value = StartIndex < Arguments.Count ? Arguments[StartIndex] : "<out of range>";
            return $"{GetType().Name}: Column[{StartIndex}] = '{value}'";
        }
    }
}
