using System.Collections.ObjectModel;
using SchemaInfoScanner.Schemata;

namespace ExcelColumnExtractor.HeaderProcessors;

public sealed class HeaderLengthContainer(ReadOnlyDictionary<RecordSchema, IReadOnlyDictionary<string, int>> headerLengths)
{
    public IReadOnlyDictionary<string, int> Get(RecordSchema rawRecordSchema)
    {
        return headerLengths[rawRecordSchema];
    }
}
