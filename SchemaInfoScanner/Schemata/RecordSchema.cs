using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.NameObjects;

namespace SchemaInfoScanner.Schemata;

public sealed record RecordSchema(
    RecordName RecordName,
    INamedTypeSymbol NamedTypeSymbol,
    ImmutableList<AttributeSyntax> RecordAttributeList,
    ImmutableList<ParameterSchemaBase> RecordParameterSchemaList)
{
    public override string ToString()
    {
        return RecordName.FullName;
    }
}
