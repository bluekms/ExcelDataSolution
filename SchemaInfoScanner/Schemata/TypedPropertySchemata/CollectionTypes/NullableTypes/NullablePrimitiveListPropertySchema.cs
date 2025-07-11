using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.Schemata.AttributeCheckers;

namespace SchemaInfoScanner.Schemata.TypedPropertySchemata.CollectionTypes.NullableTypes;

public sealed record NullablePrimitiveListPropertySchema(
    PrimitiveTypeGenericArgumentSchema GenericArgumentSchema,
    INamedTypeSymbol NamedTypeSymbol,
    IReadOnlyList<AttributeSyntax> AttributeList)
    : PropertySchemaBase(GenericArgumentSchema.PropertyName, NamedTypeSymbol, AttributeList)
{
    protected override void OnCheckCompatibility(CompatibilityContext context)
    {
        if (!context.IsCollection)
        {
            throw new InvalidOperationException($"Invalid context: {context}");
        }

        for (var i = 0; i < context.CollectionLength; i++)
        {
            var result = NullStringAttributeChecker.Check(this, context.CurrentArgument);
            if (!result.IsNull)
            {
                GenericArgumentSchema.CheckCompatibility(context);
            }
        }
    }
}
