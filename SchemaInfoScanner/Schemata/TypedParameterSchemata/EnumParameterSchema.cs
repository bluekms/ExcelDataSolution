using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.NameObjects;

namespace SchemaInfoScanner.Schemata.TypedParameterSchemata;

public sealed record EnumParameterSchema(
    ParameterName ParameterName,
    INamedTypeSymbol NamedTypeSymbol,
    ImmutableList<AttributeSyntax> AttributeList,
    IReadOnlyList<string> EnumMembers)
    : ParameterSchemaBase(ParameterName, NamedTypeSymbol, AttributeList)
{
    public override void CheckCompatibility(string argument)
    {
        if (EnumMembers.Contains(argument) is false)
        {
            throw new ArgumentException($"{argument} is not a valid enum member.");
        }
    }
}
