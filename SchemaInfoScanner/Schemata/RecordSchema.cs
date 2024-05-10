using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.NameObjects;

namespace SchemaInfoScanner.Schemata;

public sealed record RecordSchema(
    RecordName RecordName,
    INamedTypeSymbol NamedTypeSymbol,
    IReadOnlyList<AttributeSyntax> RecordAttributeList,
    IReadOnlyList<RecordParameterSchema> RecordParameterSchemaList)
{
    public bool HasAttribute<T>()
    {
        var attributeName = typeof(T).Name.Replace("Attribute", string.Empty);
        return RecordAttributeList.Any(x => x.Name.ToString() == attributeName);
    }
}
