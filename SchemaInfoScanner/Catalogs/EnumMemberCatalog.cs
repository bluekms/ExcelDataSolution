using System.Collections.ObjectModel;
using SchemaInfoScanner.Collectors;
using SchemaInfoScanner.NameObjects;

namespace SchemaInfoScanner.Catalogs;

public sealed class EnumMemberCatalog
{
    private readonly ReadOnlyDictionary<EnumName, IReadOnlyList<string>> enumMemberDictionary;

    public EnumMemberCatalog(EnumDefinitionSet enumDefinitionSet)
    {
        enumMemberDictionary = enumDefinitionSet.AsReadOnly();
    }

    public EnumMemberCatalog(RecordSchemaLoader.Result loadResult)
    {
        var enumMemberCollector = new EnumDefinitionSet(loadResult);
        enumMemberDictionary = enumMemberCollector.AsReadOnly();
    }

    public EnumMemberCatalog(IReadOnlyList<RecordSchemaLoader.Result> loadResults)
    {
        var enumMemberCollector = new EnumDefinitionSet(loadResults);
        enumMemberDictionary = enumMemberCollector.AsReadOnly();
    }

    public IReadOnlyList<string> GetEnumMembers(EnumName enumName)
    {
        return enumMemberDictionary[enumName];
    }
}
