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
    protected override void OnCheckCompatibility(CompatibilityContext context)
    {
        var result = NullStringAttributeChecker.Check(this, context.Current);
        if (result.IsNull)
        {
            context.CollectNull();
        }
        else
        {
            var actualNamedTypeSymbol = (INamedTypeSymbol)NamedTypeSymbol.TypeArguments[0];
            var schema = new EnumPropertySchema(PropertyName, actualNamedTypeSymbol, AttributeList);
            schema.CheckCompatibility(context);
        }
    }
}
