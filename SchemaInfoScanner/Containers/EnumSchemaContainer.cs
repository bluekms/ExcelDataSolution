using System.Collections.ObjectModel;
using SchemaInfoScanner.Collectors;
using SchemaInfoScanner.NameObjects;

namespace SchemaInfoScanner.Containers;

public sealed class EnumSchemaContainer
{
    private readonly ReadOnlyDictionary<EnumName, IReadOnlyList<string>> enumMemberDictionary;

    public EnumSchemaContainer(EnumMemberCollector enumMemberCollector)
    {
        enumMemberDictionary = enumMemberCollector.AsReadOnly();
    }
}
