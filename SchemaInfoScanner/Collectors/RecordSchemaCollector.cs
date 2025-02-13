using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.Exceptions;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.Schemata;

namespace SchemaInfoScanner.Collectors;

public sealed class RecordSchemaCollector
{
    private readonly Dictionary<RecordName, INamedTypeSymbol> recordNamedTypeSymbolDictionary = new();
    private readonly Dictionary<RecordName, List<AttributeSyntax>> recordAttributeDictionary = new();
    private readonly Dictionary<RecordName, List<RawParameterSchema>> recordMemberSchemaDictionary = new();

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

            var parameterSchema = new RawParameterSchema(
                parameterName,
                namedTypeSymbol,
                attributes);

            var recordName = parameterName.RecordName;
            if (recordMemberSchemaDictionary.TryGetValue(recordName, out var recordMembers))
            {
                recordMembers.Add(parameterSchema);
            }
            else
            {
                recordMemberSchemaDictionary.Add(recordName, new List<RawParameterSchema> { parameterSchema });
            }
        }
    }

    public ImmutableList<AttributeSyntax> GetRecordAttributes(RecordName recordName)
    {
        return recordAttributeDictionary[recordName].ToImmutableList();
    }

    public ImmutableList<RawParameterSchema> GetRecordMemberSchemata(RecordName recordName)
    {
        if (recordMemberSchemaDictionary.TryGetValue(recordName, out var recordMembers))
        {
            return recordMembers.ToImmutableList();
        }

        return ImmutableList<RawParameterSchema>.Empty;
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
            var namedTypeSymbol = loadResult.SemanticModel.GetDeclaredSymbol(recordDeclaration) as INamedTypeSymbol;
            if (namedTypeSymbol is null)
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
