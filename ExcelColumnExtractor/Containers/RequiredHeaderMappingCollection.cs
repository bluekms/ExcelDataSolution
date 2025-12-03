using ExcelColumnExtractor.Checkers;
using SchemaInfoScanner.Schemata;

namespace ExcelColumnExtractor.Containers;

public class RequiredHeaderMappingCollection(
    IReadOnlyDictionary<RecordSchema, RequiredHeadersChecker.RequiredHeaderMapping> targetColumnIndices)
{
    public RequiredHeadersChecker.RequiredHeaderMapping Get(RecordSchema rawRecordSchema)
    {
        return targetColumnIndices[rawRecordSchema];
    }
}
