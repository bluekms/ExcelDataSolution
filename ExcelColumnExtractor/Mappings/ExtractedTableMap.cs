using ExcelColumnExtractor.Aggregator;
using SchemaInfoScanner.Schemata;

namespace ExcelColumnExtractor.Mappings;

public sealed class ExtractedTableMap(
    IReadOnlyDictionary<RecordSchema, BodyColumnAggregator.ExtractedTable> extractedTables)
{
    public IEnumerable<KeyValuePair<RecordSchema, BodyColumnAggregator.ExtractedTable>> SortedTables =>
        extractedTables
            .OrderBy(kvp => kvp.Key.RecordName.FullName, StringComparer.Ordinal);
}
