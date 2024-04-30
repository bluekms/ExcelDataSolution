using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.NameObjects;

namespace SchemaInfoScanner.Collectors;

public sealed class ParameterNamedTypeSymbolCollector
{
    private readonly SemanticModel semanticModel;
    private readonly Dictionary<RecordParameterName, INamedTypeSymbol> namedTypeSymbolDictionary = new();

    public int Count => namedTypeSymbolDictionary.Count;

    public IEnumerable<RecordParameterName> ParameterNames => namedTypeSymbolDictionary.Keys;

    public INamedTypeSymbol this[RecordParameterName parameterName] => namedTypeSymbolDictionary[parameterName];

    public ParameterNamedTypeSymbolCollector(SemanticModel semanticModel)
    {
        this.semanticModel = semanticModel;
    }

    public void Collect(RecordDeclarationSyntax recordDeclaration, ParameterSyntax parameter)
    {
        if (parameter.Type is null)
        {
            throw new NotSupportedException();
        }

        var typeSymbol = semanticModel.GetTypeInfo(parameter.Type).Type;
        if (typeSymbol is not INamedTypeSymbol namedTypeSymbol)
        {
            throw new NotSupportedException();
        }

        var recordPropertyName = new RecordParameterName(new RecordName(recordDeclaration), parameter);
        namedTypeSymbolDictionary.Add(recordPropertyName, namedTypeSymbol);
    }
}
