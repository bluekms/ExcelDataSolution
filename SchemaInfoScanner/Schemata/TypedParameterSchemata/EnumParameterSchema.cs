using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.NameObjects;

namespace SchemaInfoScanner.Schemata.TypedParameterSchemata;

public sealed record EnumParameterSchema(
    ParameterName ParameterName,
    INamedTypeSymbol NamedTypeSymbol,
    IReadOnlyList<AttributeSyntax> AttributeList)
    : ParameterSchemaBase(ParameterName, NamedTypeSymbol, AttributeList)
{
    public override void CheckCompatibility(string argument)
    {
        throw new InvalidOperationException($"{ParameterName.FullName} is enum. Use CheckCompatibility(string, EnumMemberContainer) instead.");
    }

    public void CheckCompatibility(string argument, EnumMemberContainer enumMemberContainer)
    {
        /*
        if (EnumMembers.Contains(argument) is false)
        {
            throw new ArgumentException($"{argument} is not a valid enum member.");
        }
        */
    }
}
