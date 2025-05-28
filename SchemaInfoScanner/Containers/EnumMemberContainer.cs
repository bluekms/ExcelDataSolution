using System.Collections.ObjectModel;
using SchemaInfoScanner.Collectors;
using SchemaInfoScanner.NameObjects;

namespace SchemaInfoScanner.Containers;

public sealed class EnumMemberContainer
{
    private readonly ReadOnlyDictionary<EnumName, IReadOnlyList<string>> enumMemberDictionary;

    public EnumMemberContainer(EnumDefinitionSet enumDefinitionSet)
    {
        enumMemberDictionary = enumDefinitionSet.AsReadOnly();
    }

    public EnumMemberContainer(RecordSchemaLoader.Result loadResult)
    {
        var enumMemberCollector = new EnumDefinitionSet(loadResult);
        enumMemberDictionary = enumMemberCollector.AsReadOnly();
    }

    public IReadOnlyList<string> GetEnumMembers(EnumName enumName)
    {
        return enumMemberDictionary[enumName];
    }
}
