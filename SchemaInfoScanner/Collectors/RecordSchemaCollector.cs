using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.Exceptions;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.Schemata;

namespace SchemaInfoScanner.Collectors;

public sealed class RecordSchemaCollector
{
    private readonly Dictionary<RecordName, INamedTypeSymbol> recordNamedTypeSymbolDictionary = [];
    private readonly Dictionary<RecordName, List<AttributeSyntax>> recordAttributeDictionary = [];
    private readonly Dictionary<RecordName, List<RawParameterSchema>> recordMemberSchemaDictionary = [];

    public int Count => recordAttributeDictionary.Count;

    public IReadOnlyList<RecordName> RecordNames => recordAttributeDictionary.Keys.ToList();

    public RecordSchemaCollector()
    {
    }

    public RecordSchemaCollector(RecordSchemaLoader.Result loadResult)
    {
        Collect(loadResult);
    }

    public void Collect(RecordSchemaLoader.Result loadResult)
    {
        var result = Parse(loadResult);

        foreach (var (recordName, namedTypeSymbol) in result.RecordNamedTypeSymbolCollector)
        {
            recordNamedTypeSymbolDictionary.Add(recordName, namedTypeSymbol);
        }

        foreach (var (recordName, recordAttributes) in result.RecordAttributeCollector)
        {
            recordAttributeDictionary.Add(recordName, recordAttributes.ToList());
        }

        foreach (var parameterName in result.ParameterNamedTypeSymbolCollector.ParameterNames)
        {
            var namedTypeSymbol = result.ParameterNamedTypeSymbolCollector[parameterName];
            var attributes = result.ParameterAttributeCollector[parameterName];

            var location = new LocationInfo(
                namedTypeSymbol.ContainingNamespace?.ToDisplayString() ?? string.Empty,
                GetParentContainers(namedTypeSymbol.ContainingType),
                namedTypeSymbol.ContainingType?.Name ?? string.Empty);

            var parameterSchema = new RawParameterSchema(
                parameterName,
                namedTypeSymbol,
                attributes,
                location);

            var recordName = parameterName.RecordName;
            if (recordMemberSchemaDictionary.TryGetValue(recordName, out var recordMembers))
            {
                recordMembers.Add(parameterSchema);
            }
            else
            {
                recordMemberSchemaDictionary.Add(recordName, [parameterSchema]);
            }
        }
    }

    private static IReadOnlyList<string> GetParentContainers(INamedTypeSymbol? containingType)
    {
        var containers = new List<string>();

        while (containingType != null)
        {
            containers.Add(containingType.Name);
            containingType = containingType.ContainingType; // 부모 클래스/컨테이너로 이동
        }

        containers.Reverse(); // 부모 클래스부터 자식 클래스까지 순서대로 저장되도록 역순으로 변경
        return containers;
    }

    public IReadOnlyList<AttributeSyntax> GetRecordAttributes(RecordName recordName)
    {
        return recordAttributeDictionary[recordName];
    }

    public IReadOnlyList<RawParameterSchema> GetRecordMemberSchemata(RecordName recordName)
    {
        if (recordMemberSchemaDictionary.TryGetValue(recordName, out var recordMembers))
        {
            return recordMembers;
        }

        return [];
    }

    public INamedTypeSymbol GetNamedTypeSymbol(RecordName recordName)
    {
        return recordNamedTypeSymbolDictionary[recordName];
    }

    private static ParseResult Parse(RecordSchemaLoader.Result loadResult)
    {
        var recordNamedTypeSymbolCollector = new Dictionary<RecordName, INamedTypeSymbol>();
        var recordAttributeCollector = new RecordAttributeCollector();
        var parameterAttributeCollector = new ParameterAttributeCollector();
        var parameterNamedTypeSymbolCollector = new ParameterNamedTypeSymbolCollector(loadResult.SemanticModel);

        foreach (var recordDeclaration in loadResult.RecordDeclarationList)
        {
            var recordName = new RecordName(recordDeclaration);
            if (loadResult.SemanticModel.GetDeclaredSymbol(recordDeclaration) is not INamedTypeSymbol namedTypeSymbol)
            {
                throw new TypeNotSupportedException($"{recordName.FullName} is not a named type symbol");
            }

            recordNamedTypeSymbolCollector.Add(recordName, namedTypeSymbol);

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

        return new(
            recordNamedTypeSymbolCollector,
            recordAttributeCollector,
            parameterAttributeCollector,
            parameterNamedTypeSymbolCollector);
    }

    private sealed class ParseResult
    {
        public Dictionary<RecordName, INamedTypeSymbol> RecordNamedTypeSymbolCollector { get; }
        public RecordAttributeCollector RecordAttributeCollector { get; }
        public ParameterAttributeCollector ParameterAttributeCollector { get; }
        public ParameterNamedTypeSymbolCollector ParameterNamedTypeSymbolCollector { get; }

        public ParseResult(
            Dictionary<RecordName, INamedTypeSymbol> recordNamedTypeSymbolCollector,
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

            RecordNamedTypeSymbolCollector = recordNamedTypeSymbolCollector;
            RecordAttributeCollector = recordAttributeCollector;
            ParameterAttributeCollector = parameterAttributeCollector;
            ParameterNamedTypeSymbolCollector = parameterNamedTypeSymbolCollector;
        }
    }
}
