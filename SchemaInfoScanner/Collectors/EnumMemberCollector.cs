using System.Collections.Frozen;
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

    public FrozenDictionary<EnumName, IReadOnlyList<string>> ToFrozenDictionary()
    {
        return enumMemberDictionary.ToFrozenDictionary();
    }
}
