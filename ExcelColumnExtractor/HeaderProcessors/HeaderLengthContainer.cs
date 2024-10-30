using System.Collections.ObjectModel;
using SchemaInfoScanner.Schemata;

namespace ExcelColumnExtractor.HeaderProcessors;

public sealed class HeaderLengthContainer(ReadOnlyDictionary<RawRecordSchema, IReadOnlyDictionary<string, int>> headerLengths)
{
    public IReadOnlyDictionary<string, int> Get(RawRecordSchema rawRecordSchema)
    {
        return headerLengths[rawRecordSchema];
    }
}
