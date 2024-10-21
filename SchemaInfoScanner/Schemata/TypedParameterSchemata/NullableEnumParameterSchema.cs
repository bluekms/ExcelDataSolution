using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.Schemata.AttributeCheckers;

namespace SchemaInfoScanner.Schemata.TypedParameterSchemata;

public sealed record NullableEnumParameterSchema(
    ParameterName ParameterName,
    INamedTypeSymbol NamedTypeSymbol,
    IReadOnlyList<AttributeSyntax> AttributeList)
    : ParameterSchemaBase(ParameterName, NamedTypeSymbol, AttributeList)
{
    public override void CheckCompatibility(string argument)
    {
        throw new InvalidOperationException($"{ParameterName.FullName} is nullable enum. Use CheckCompatibility(string, EnumMemberContainer) instead.");
    }

    public void CheckCompatibility(string argument, EnumMemberContainer enumMemberContainer)
    {
        var result = NullStringAttributeChecker.Check(this, argument);
        if (result.IsNull)
        {
            return;
        }

        var schema = new EnumParameterSchema(ParameterName, NamedTypeSymbol, AttributeList);
        schema.CheckCompatibility(argument, enumMemberContainer);
    }
}
