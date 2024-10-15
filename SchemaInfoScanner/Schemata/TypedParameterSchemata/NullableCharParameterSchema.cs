using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.Schemata.AttributeCheckers;

namespace SchemaInfoScanner.Schemata.TypedParameterSchemata;

public sealed record NullableCharParameterSchema(
    ParameterName ParameterName,
    INamedTypeSymbol NamedTypeSymbol,
    ImmutableList<AttributeSyntax> AttributeList)
    : ParameterSchemaBase(ParameterName, NamedTypeSymbol, AttributeList)
{
    public override void CheckCompatibility(string argument)
    {
        var result = NullStringAttributeChecker.Check(this, argument);
        if (result.IsNull)
        {
            return;
        }

        var schema = new CharParameterSchema(ParameterName, NamedTypeSymbol, AttributeList);
        schema.CheckCompatibility(argument);
    }
}
