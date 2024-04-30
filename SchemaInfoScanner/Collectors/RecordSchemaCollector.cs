using System.Collections.Frozen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.NameObjects;

namespace SchemaInfoScanner.Collectors;

public sealed record RecordParameterSchema(
    RecordParameterName ParameterName,
    INamedTypeSymbol NamedTypeSymbol,
    IReadOnlyList<AttributeSyntax> Attributes);

public sealed partial class RecordSchemaCollector
{
    private readonly Dictionary<RecordName, List<AttributeSyntax>> recordAttributeDictionary = new();
    private readonly Dictionary<RecordName, List<RecordParameterSchema>> recordMemberSchemaDictionary = new();
    private readonly Dictionary<EnumName, IReadOnlyList<string>> enumMemberDictionary = new();

    public int Count => recordAttributeDictionary.Count;

    public IReadOnlyList<RecordName> RecordNames => recordAttributeDictionary.Keys.ToList();

    public void Collect(LoadResult loadResult)
    {
        var result = LoadResultParser.Parse(loadResult);

        foreach (var (recordName, recordAttributes) in result.RecordAttributeCollector)
        {
            recordAttributeDictionary.Add(recordName, recordAttributes.ToList());
        }

        foreach (var parameterName in result.ParameterNamedTypeSymbolCollector.ParameterNames)
        {
            var recordName = parameterName.RecordName;
            var namedTypeSymbol = result.ParameterNamedTypeSymbolCollector[parameterName];
            var attributes = result.ParameterAttributeCollector[parameterName];

            if (recordMemberSchemaDictionary.TryGetValue(recordName, out var recordMembers))
            {
                recordMembers.Add(new(parameterName, namedTypeSymbol, attributes.ToList()));
            }
            else
            {
                recordMemberSchemaDictionary.Add(recordName, new List<RecordParameterSchema> { new(parameterName, namedTypeSymbol, attributes.ToList()) });
            }
        }

        foreach (var (enumName, enumMemberList) in result.EnumMemberCollector)
        {
            enumMemberDictionary.Add(enumName, enumMemberList);
        }
    }

    public IReadOnlyList<AttributeSyntax> GetRecordAttributes(RecordName recordName)
    {
        return recordAttributeDictionary[recordName];
    }

    public IReadOnlyList<RecordParameterSchema> GetRecordMemberSchemata(RecordName recordName)
    {
        if (recordMemberSchemaDictionary.TryGetValue(recordName, out var recordMembers))
        {
            return recordMembers;
        }

        return Array.Empty<RecordParameterSchema>();
    }

    public FrozenDictionary<EnumName, IReadOnlyList<string>> GetEnumMemberFrozenDictionary()
    {
        return enumMemberDictionary.ToFrozenDictionary();
    }
}
