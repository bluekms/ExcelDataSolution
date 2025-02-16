using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.NameObjects;

namespace SchemaInfoScanner.Collectors;

public sealed class ParameterAttributeCollector
{
    private readonly Dictionary<ParameterName, IReadOnlyList<AttributeSyntax>> attributesDictionary = new();

    public int Count => attributesDictionary.Count;

    public bool ContainsRecord(ParameterName parameterName) => attributesDictionary.ContainsKey(parameterName);

    public IReadOnlyList<AttributeSyntax> this[ParameterName parameterName] => attributesDictionary[parameterName];

    public void Collect(RecordDeclarationSyntax recordDeclaration, ParameterSyntax parameter)
    {
        var recordPropertyName = new ParameterName(new RecordName(recordDeclaration), parameter);
        var attributeList = parameter.AttributeLists.SelectMany(x => x.Attributes).ToList().AsReadOnly();

        attributesDictionary.Add(recordPropertyName, attributeList);
    }
}
