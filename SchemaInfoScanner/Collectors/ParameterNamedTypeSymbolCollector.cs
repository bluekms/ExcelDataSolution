using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.Exceptions;
using SchemaInfoScanner.NameObjects;

namespace SchemaInfoScanner.Collectors;

public sealed class ParameterNamedTypeSymbolCollector(SemanticModel semanticModel)
{
    private readonly Dictionary<PropertyName, INamedTypeSymbol> namedTypeSymbolDictionary = [];

    public int Count => namedTypeSymbolDictionary.Count;

    public IEnumerable<PropertyName> ParameterNames => namedTypeSymbolDictionary.Keys;

    public INamedTypeSymbol this[PropertyName propertyName] => namedTypeSymbolDictionary[propertyName];

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

        var recordPropertyName = new PropertyName(new RecordName(recordDeclaration), parameter);
        namedTypeSymbolDictionary.Add(recordPropertyName, namedTypeSymbol);
    }
}
