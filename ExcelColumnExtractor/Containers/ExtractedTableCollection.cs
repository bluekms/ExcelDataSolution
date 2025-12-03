using ExcelColumnExtractor.Aggregator;
using SchemaInfoScanner.Schemata;

namespace ExcelColumnExtractor.Containers;

public sealed class ExtractedTableCollection(
    IReadOnlyDictionary<RecordSchema, BodyColumnAggregator.ExtractedTable> extractedTables)
{
    public IEnumerable<KeyValuePair<RecordSchema, BodyColumnAggregator.ExtractedTable>> SortedTables =>
        extractedTables
            .OrderBy(kvp => kvp.Key.RecordName.FullName, StringComparer.Ordinal);
}
