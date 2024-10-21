using ExcelColumnExtractor.Aggregator;
using SchemaInfoScanner.Schemata;

namespace ExcelColumnExtractor.Containers;

public sealed class ExtractedTableContainer(
    IReadOnlyDictionary<RawRecordSchema, BodyColumnAggregator.ExtractedTable> extractedTables)
{
    public IEnumerable<KeyValuePair<RawRecordSchema, BodyColumnAggregator.ExtractedTable>> SortedTables =>
        extractedTables
            .OrderBy(kvp => kvp.Key.RecordName.FullName, StringComparer.Ordinal);
}
