using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.Schemata.AttributeCheckers;

namespace SchemaInfoScanner.Schemata.TypedPropertySchemata.PrimitiveTypes.NullableTypes;

public sealed record NullableDecimalPropertySchema(
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
            context.Collect(null);
            return 1;
        }

        var schema = new DecimalPropertySchema(PropertyName, NamedTypeSymbol, AttributeList);
        return schema.CheckCompatibility(context);
    }
}
