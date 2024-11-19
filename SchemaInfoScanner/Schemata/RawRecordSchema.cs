using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.NameObjects;

namespace SchemaInfoScanner.Schemata;

public sealed record RawRecordSchema(
    RecordName RecordName,
    INamedTypeSymbol NamedTypeSymbol,
    IReadOnlyList<AttributeSyntax> RecordAttributeList,
    IReadOnlyList<RawParameterSchema> RawParameterSchemaList)
{
    public override string ToString()
    {
        return $"(Raw) {RecordName.FullName}";
    }
}
