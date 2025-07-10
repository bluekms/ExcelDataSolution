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
    protected override int OnCheckCompatibility(CompatibilityContext context)
    {
        if (!context.IsCollection)
        {
            throw new InvalidOperationException($"Invalid context: {context}");
        }

        var totalConsumed = 0;
        for (var i = 0; i < context.CollectionLength; i++)
        {
            var nestedContext = context.WithStartIndex(context.StartIndex + totalConsumed);

            var result = NullStringAttributeChecker.Check(this, nestedContext.CurrentArgument);
            if (result.IsNull)
            {
                totalConsumed += 1;
            }
            else
            {
                totalConsumed += GenericArgumentSchema.CheckCompatibility(nestedContext);
            }
        }

        return totalConsumed;
    }
}
