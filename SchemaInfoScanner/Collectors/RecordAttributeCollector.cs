using System.Collections;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.NameObjects;

namespace SchemaInfoScanner.Collectors;

public class RecordAttributeCollector : IEnumerable<KeyValuePair<RecordName, IReadOnlyList<AttributeSyntax>>>
{
    private readonly Dictionary<RecordName, IReadOnlyList<AttributeSyntax>> attributesDictionary = [];

    public int Count => attributesDictionary.Count;

    public void Collect(RecordDeclarationSyntax recordDeclaration)
    {
        var attributeList = recordDeclaration.AttributeLists
            .SelectMany(x => x.Attributes)
            .ToList()
            .AsReadOnly();

        attributesDictionary.Add(new RecordName(recordDeclaration), attributeList);
    }

    public IEnumerator<KeyValuePair<RecordName, IReadOnlyList<AttributeSyntax>>> GetEnumerator()
    {
        return attributesDictionary.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
