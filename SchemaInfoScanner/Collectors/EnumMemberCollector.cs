using System.Collections.ObjectModel;
using SchemaInfoScanner.NameObjects;

namespace SchemaInfoScanner.Collectors;

public class EnumMemberCollector
{
    private readonly Dictionary<EnumName, IReadOnlyList<string>> enumMemberDictionary = new();

    public EnumMemberCollector()
    {
    }

    public void Collect(RecordSchemaLoader.Result loadResult)
    {
        foreach (var enumDeclaration in loadResult.EnumDeclarationList)
        {
            var enumName = new EnumName(enumDeclaration);
            var members = enumDeclaration.Members.Select(x => x.Identifier.ValueText).ToList();
            enumMemberDictionary.Add(enumName, members);
        }
    }

    public ReadOnlyDictionary<EnumName, IReadOnlyList<string>> AsReadOnly()
    {
        return enumMemberDictionary.AsReadOnly();
    }
}
