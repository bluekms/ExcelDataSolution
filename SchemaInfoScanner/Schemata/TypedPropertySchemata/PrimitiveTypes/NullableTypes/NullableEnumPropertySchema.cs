using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.Schemata.AttributeCheckers;

namespace SchemaInfoScanner.Schemata.TypedPropertySchemata.PrimitiveTypes.NullableTypes;

public sealed record NullableEnumPropertySchema(
    PropertyName PropertyName,
    INamedTypeSymbol NamedTypeSymbol,
    IReadOnlyList<AttributeSyntax> AttributeList)
    : PropertySchemaBase(PropertyName, NamedTypeSymbol, AttributeList)
{
    protected override int OnCheckCompatibility(CompatibilityContext context)
    {
        var argument = context.CurrentArgument;
        var result = NullStringAttributeChecker.Check(this, argument);
        if (result.IsNull)
        {
            return 1;
        }

        var actualNamedTypeSymbol = (INamedTypeSymbol)NamedTypeSymbol.TypeArguments[0];
        var schema = new EnumPropertySchema(PropertyName, actualNamedTypeSymbol, AttributeList);
        return schema.CheckCompatibility(context);
    }
}
