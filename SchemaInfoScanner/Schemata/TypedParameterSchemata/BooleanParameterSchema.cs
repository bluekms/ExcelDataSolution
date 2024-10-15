using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.NameObjects;

namespace SchemaInfoScanner.Schemata.TypedParameterSchemata;

public sealed record BooleanParameterSchema(
    ParameterName ParameterName,
    INamedTypeSymbol NamedTypeSymbol,
    ImmutableList<AttributeSyntax> AttributeList)
    : ParameterSchemaBase(ParameterName, NamedTypeSymbol, AttributeList)
{
    public override void CheckCompatibility(string argument)
    {
        _ = bool.Parse(argument);
    }
}
