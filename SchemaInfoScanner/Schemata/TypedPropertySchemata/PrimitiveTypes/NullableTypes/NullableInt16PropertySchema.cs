using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.Schemata.AttributeCheckers;

namespace SchemaInfoScanner.Schemata.TypedPropertySchemata.PrimitiveTypes.NullableTypes;

public sealed record NullableInt16PropertySchema(
    PropertyName PropertyName,
    INamedTypeSymbol NamedTypeSymbol,
    IReadOnlyList<AttributeSyntax> AttributeList)
    : PropertySchemaBase(PropertyName, NamedTypeSymbol, AttributeList)
{
    protected override void OnCheckCompatibility(CompatibilityContext context)
    {
        var result = NullStringAttributeChecker.Check(this, context.Current.Value);
        if (result.IsNull)
        {
            context.ConsumeNull();
        }
        else
        {
            var schema = new Int16PropertySchema(PropertyName, NamedTypeSymbol, AttributeList);
            schema.CheckCompatibility(context);
        }
    }
}
