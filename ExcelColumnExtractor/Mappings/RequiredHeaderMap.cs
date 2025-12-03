using ExcelColumnExtractor.Checkers;
using SchemaInfoScanner.Schemata;

namespace ExcelColumnExtractor.Mappings;

public class RequiredHeaderMap(
    IReadOnlyDictionary<RecordSchema, RequiredHeadersChecker.RequiredHeaderMapping> targetColumnIndices)
{
    public RequiredHeadersChecker.RequiredHeaderMapping Get(RecordSchema rawRecordSchema)
    {
        return targetColumnIndices[rawRecordSchema];
    }
}
