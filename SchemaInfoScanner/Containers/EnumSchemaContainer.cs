using System.Collections.Frozen;
using System.Collections.Immutable;
using SchemaInfoScanner.Collectors;
using SchemaInfoScanner.NameObjects;

namespace SchemaInfoScanner.Containers;

public sealed class EnumSchemaContainer
{
    private readonly FrozenDictionary<EnumName, ImmutableList<string>> enumMemberDictionary;

    public EnumSchemaContainer(EnumMemberCollector enumMemberCollector)
    {
        enumMemberDictionary = enumMemberCollector.ToFrozenDictionary();
    }
}
