using System.Collections;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.NameObjects;

namespace SchemaInfoScanner.Collectors;

public class EnumMemberCollector : IEnumerable<KeyValuePair<EnumName, IReadOnlyList<string>>>
{
    private readonly Dictionary<EnumName, IReadOnlyList<string>> enumMemberDictionary = new();

    public int Count => enumMemberDictionary.Count;

    public void Collect(EnumDeclarationSyntax enumDeclaration)
    {
        var enumName = new EnumName(enumDeclaration);
        var members = enumDeclaration.Members.Select(x => x.Identifier.ValueText).ToList();
        enumMemberDictionary.Add(enumName, members);
    }

    public IEnumerator<KeyValuePair<EnumName, IReadOnlyList<string>>> GetEnumerator()
    {
        return enumMemberDictionary.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
