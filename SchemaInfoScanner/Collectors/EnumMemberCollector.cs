using System.Collections.Frozen;
using System.Collections.Immutable;
using SchemaInfoScanner.NameObjects;

namespace SchemaInfoScanner.Collectors;

public class EnumMemberCollector
{
    private readonly Dictionary<EnumName, IReadOnlyList<string>> enumMemberDictionary = new();

    public void Collect(LoadResult loadResult)
    {
        foreach (var enumDeclaration in loadResult.EnumDeclarationList)
        {
            var enumName = new EnumName(enumDeclaration);
            var members = enumDeclaration.Members.Select(x => x.Identifier.ValueText).ToList();
            enumMemberDictionary.Add(enumName, members);
        }
    }

    public FrozenDictionary<EnumName, ImmutableList<string>> ToFrozenDictionary()
    {
        return enumMemberDictionary
            .ToDictionary(pair => pair.Key, pair => pair.Value.ToImmutableList())
            .ToFrozenDictionary();
    }
}
