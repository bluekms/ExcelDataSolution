using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SchemaInfoScanner.Collectors;

public sealed record RecordParameterSchema(
    RecordParameterName ParameterName,
    INamedTypeSymbol NamedTypeSymbol,
    IReadOnlyList<AttributeSyntax> Attributes);

public sealed class RecordSchemaCollector
{
    private readonly Dictionary<RecordName, List<AttributeSyntax>> recordAttributeDictionary = new();
    private readonly Dictionary<RecordName, List<RecordParameterSchema>> recordMemberSchemaDictionary = new();

    public int Count => recordAttributeDictionary.Count;

    public IReadOnlyList<RecordName> RecordNames => recordAttributeDictionary.Keys.ToList();

    public void Collect(LoadResult loadResult)
    {
        var result = OnCollect(loadResult);

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

    private sealed class OnCollectResult
    {
        public RecordAttributeCollector RecordAttributeCollector { get; }
        public ParameterAttributeCollector ParameterAttributeCollector { get; }
        public ParameterNamedTypeSymbolCollector ParameterNamedTypeSymbolCollector { get; }

        public OnCollectResult(
            RecordAttributeCollector recordAttributeCollector,
            ParameterAttributeCollector parameterAttributeCollector,
            ParameterNamedTypeSymbolCollector parameterNamedTypeSymbolCollector)
        {
            if (parameterNamedTypeSymbolCollector.Count != parameterAttributeCollector.Count)
            {
                throw new ArgumentException("Count mismatch");
            }

            foreach (var parameterFullName in parameterNamedTypeSymbolCollector.ParameterNames)
            {
                if (!parameterAttributeCollector.ContainsRecord(parameterFullName))
                {
                    throw new ArgumentException($"{parameterFullName} not found");
                }
            }

            RecordAttributeCollector = recordAttributeCollector;
            ParameterAttributeCollector = parameterAttributeCollector;
            ParameterNamedTypeSymbolCollector = parameterNamedTypeSymbolCollector;
        }
    }

    private static OnCollectResult OnCollect(LoadResult loadResult)
    {
        var recordAttributeCollector = new RecordAttributeCollector();
        var parameterAttributeCollector = new ParameterAttributeCollector();
        var parameterNamedTypeSymbolCollector = new ParameterNamedTypeSymbolCollector(loadResult.SemanticModel);

        foreach (var recordDeclaration in loadResult.RecordDeclarationList)
        {
            recordAttributeCollector.Collect(recordDeclaration);

            if (recordDeclaration.ParameterList is null)
            {
                continue;
            }

            foreach (var parameter in recordDeclaration.ParameterList.Parameters)
            {
                if (string.IsNullOrEmpty(parameter.Identifier.ValueText))
                {
                    continue;
                }

                parameterAttributeCollector.Collect(recordDeclaration, parameter);
                parameterNamedTypeSymbolCollector.Collect(recordDeclaration, parameter);
            }
        }

        return new(recordAttributeCollector, parameterAttributeCollector, parameterNamedTypeSymbolCollector);
    }
}