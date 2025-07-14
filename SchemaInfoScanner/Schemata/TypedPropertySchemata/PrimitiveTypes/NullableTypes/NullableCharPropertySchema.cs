using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.Schemata.AttributeCheckers;

namespace SchemaInfoScanner.Schemata.TypedPropertySchemata.PrimitiveTypes.NullableTypes;

public sealed record NullableCharPropertySchema(
    PropertyName PropertyName,
    INamedTypeSymbol NamedTypeSymbol,
    IReadOnlyList<AttributeSyntax> AttributeList)
    : PropertySchemaBase(PropertyName, NamedTypeSymbol, AttributeList)
{
    protected override void OnCheckCompatibility(CompatibilityContext context)
    {
        var argument = context.CurrentArgument;
        var result = NullStringAttributeChecker.Check(this, argument);
        if (result.IsNull)
        {
            context.Collect(null);
        }
        else
        {
            var schema = new CharPropertySchema(PropertyName, NamedTypeSymbol, AttributeList);
            schema.CheckCompatibility(context);
        }
    }
}
