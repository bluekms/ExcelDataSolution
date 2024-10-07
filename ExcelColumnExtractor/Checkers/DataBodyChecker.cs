using ExcelColumnExtractor.Aggregator;
using ExcelColumnExtractor.Containers;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.Schemata;

namespace ExcelColumnExtractor.Checkers;

public static class DataBodyChecker
{
    public static void Check(
        IReadOnlyList<RecordSchema> staticDataRecordSchemaList,
        RecordSchemaContainer recordSchemaContainer,
        ExtractedTableContainer extractedTableContainer,
        ILogger<Program> logger)
    {
    }
}
