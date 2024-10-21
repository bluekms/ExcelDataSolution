using ExcelColumnExtractor.Checkers;
using SchemaInfoScanner.Schemata;

namespace ExcelColumnExtractor.Containers;

public class TargetColumnIndicesContainer(
    IReadOnlyDictionary<RawRecordSchema, RequiredHeadersChecker.TargetColumnIndices> targetColumnIndices)
{
    public RequiredHeadersChecker.TargetColumnIndices Get(RawRecordSchema rawRecordSchema)
    {
        return targetColumnIndices[rawRecordSchema];
    }
}
