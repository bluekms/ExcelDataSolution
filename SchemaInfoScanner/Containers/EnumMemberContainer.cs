using System.Collections.ObjectModel;
using Microsoft.CodeAnalysis;
using SchemaInfoScanner.Collectors;
using SchemaInfoScanner.NameObjects;

namespace SchemaInfoScanner.Containers;

public sealed class EnumMemberContainer
{
    private readonly ReadOnlyDictionary<EnumName, IReadOnlyList<string>> enumMemberDictionary;

    public EnumMemberContainer(EnumMemberCollector enumMemberCollector)
    {
        enumMemberDictionary = enumMemberCollector.AsReadOnly();
    }

    public EnumMemberContainer(RecordSchemaLoader.Result loadResult)
    {
        Collect(loadResult);
    }

    public void Collect(RecordSchemaLoader.Result loadResult)
    {
        var result = ParseOptions(l)
    }
}
