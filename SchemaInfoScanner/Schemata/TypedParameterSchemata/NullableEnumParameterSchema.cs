using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.Schemata.AttributeCheckers;

namespace SchemaInfoScanner.Schemata.TypedParameterSchemata;

public sealed record NullableEnumParameterSchema(
    ParameterName ParameterName,
    INamedTypeSymbol NamedTypeSymbol,
    ImmutableList<AttributeSyntax> AttributeList,
    IReadOnlyList<string> EnumMembers)
    : ParameterSchemaBase(ParameterName, NamedTypeSymbol, AttributeList)
{
    public override void CheckCompatibility(string argument)
    {
        var result = NullStringAttributeChecker.Check(this, argument);
        if (result.IsNull)
        {
            return;
        }

        var schema = new EnumParameterSchema(ParameterName, NamedTypeSymbol, AttributeList, EnumMembers);
        schema.CheckCompatibility(argument);
    }
}
