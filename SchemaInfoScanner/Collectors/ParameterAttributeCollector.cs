using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.NameObjects;

namespace SchemaInfoScanner.Collectors;

public sealed class ParameterAttributeCollector
{
    private readonly Dictionary<PropertyName, IReadOnlyList<AttributeSyntax>> attributesDictionary = [];

    public int Count => attributesDictionary.Count;

    public bool ContainsRecord(PropertyName propertyName)
    {
        return attributesDictionary.ContainsKey(propertyName);
    }

    public IReadOnlyList<AttributeSyntax> this[PropertyName propertyName] => attributesDictionary[propertyName];

    public void Collect(RecordDeclarationSyntax recordDeclaration, ParameterSyntax parameter)
    {
        var recordPropertyName = new PropertyName(new RecordName(recordDeclaration), parameter);
        var attributeList = parameter.AttributeLists.SelectMany(x => x.Attributes).ToList().AsReadOnly();

        attributesDictionary.Add(recordPropertyName, attributeList);
    }
}
