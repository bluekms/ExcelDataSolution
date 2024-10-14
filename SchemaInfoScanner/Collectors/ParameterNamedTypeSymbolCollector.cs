using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.Exceptions;
using SchemaInfoScanner.NameObjects;

namespace SchemaInfoScanner.Collectors;

public sealed class ParameterNamedTypeSymbolCollector
{
    private readonly SemanticModel semanticModel;
    private readonly Dictionary<ParameterName, INamedTypeSymbol> namedTypeSymbolDictionary = new();

    public int Count => namedTypeSymbolDictionary.Count;

    public IEnumerable<ParameterName> ParameterNames => namedTypeSymbolDictionary.Keys;

    public INamedTypeSymbol this[ParameterName parameterName] => namedTypeSymbolDictionary[parameterName];

    public ParameterNamedTypeSymbolCollector(SemanticModel semanticModel)
    {
        this.semanticModel = semanticModel;
    }

    public void Collect(RecordDeclarationSyntax recordDeclaration, ParameterSyntax parameter)
    {
        if (parameter.Type is null)
        {
            throw new TypeNotSupportedException();
        }

        var typeSymbol = semanticModel.GetTypeInfo(parameter.Type).Type;
        if (typeSymbol is not INamedTypeSymbol namedTypeSymbol)
        {
            throw new TypeNotSupportedException();
        }

        var recordPropertyName = new ParameterName(new RecordName(recordDeclaration), parameter);
        namedTypeSymbolDictionary.Add(recordPropertyName, namedTypeSymbol);
    }
}
