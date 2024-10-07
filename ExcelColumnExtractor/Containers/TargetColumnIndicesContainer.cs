using ExcelColumnExtractor.Checkers;
using SchemaInfoScanner.Schemata;

namespace ExcelColumnExtractor.Containers;

public class TargetColumnIndicesContainer(
    IReadOnlyDictionary<RecordSchema, RequiredHeadersChecker.TargetColumnIndices> targetColumnIndices)
{
    public RequiredHeadersChecker.TargetColumnIndices Get(RecordSchema recordSchema)
    {
        return targetColumnIndices[recordSchema];
    }
}
