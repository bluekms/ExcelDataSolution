using System.Collections.Frozen;
using SchemaInfoScanner.Collectors;
using SchemaInfoScanner.NameObjects;

namespace SchemaInfoScanner.Containers;

public sealed class EnumSchemaContainer
{
    private readonly FrozenDictionary<EnumName, IReadOnlyList<string>> enumMemberDictionary;

    public EnumSchemaContainer(EnumMemberCollector enumMemberCollector)
    {
        enumMemberDictionary = enumMemberCollector.ToFrozenDictionary();
    }
}
