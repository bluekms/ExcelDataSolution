using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.NameObjects;

namespace SchemaInfoScanner.Collectors;

public sealed class ParameterAttributeCollector
{
    private readonly Dictionary<RecordParameterName, IReadOnlyList<AttributeSyntax>> attributesDictionary = new();

    public int Count => attributesDictionary.Count;

    public bool ContainsRecord(RecordParameterName parameterName) => attributesDictionary.ContainsKey(parameterName);

    public IReadOnlyList<AttributeSyntax> this[RecordParameterName parameterName] => attributesDictionary[parameterName];

    public void Collect(RecordDeclarationSyntax recordDeclaration, ParameterSyntax parameter)
    {
        var recordPropertyName = new RecordParameterName(new RecordName(recordDeclaration), parameter);
        var attributeList = parameter.AttributeLists.SelectMany(x => x.Attributes).ToList().AsReadOnly();

        attributesDictionary.Add(recordPropertyName, attributeList);
    }
}
