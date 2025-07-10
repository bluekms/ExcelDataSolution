using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.Schemata.AttributeCheckers;

namespace SchemaInfoScanner.Schemata.TypedPropertySchemata.CollectionTypes.NullableTypes;

public sealed record PrimitiveKeyNullablePrimitiveValueDictionaryPropertySchema(
    PropertyName PropertyName,
    INamedTypeSymbol NamedTypeSymbol,
    IReadOnlyList<AttributeSyntax> AttributeList,
    PrimitiveTypeGenericArgumentSchema KeySchema,
    PrimitiveTypeGenericArgumentSchema ValueSchema)
    : PropertySchemaBase(PropertyName, NamedTypeSymbol, AttributeList)
{
    protected override int OnCheckCompatibility(CompatibilityContext context)
    {
        if (!context.IsCollection)
        {
            throw new InvalidOperationException($"Invalid context: {context}");
        }

        var consumedCount = 0;
        for (var i = 0; i < context.CollectionLength; i++)
        {
            var keyConsumed = KeySchema.CheckCompatibility(context);
            context.StartIndex += keyConsumed;
            consumedCount += keyConsumed;

            var result = NullStringAttributeChecker.Check(this, context.CurrentArgument);
            if (!result.IsNull)
            {
                var valueConsumed = ValueSchema.CheckCompatibility(context);
                context.StartIndex += valueConsumed;
                consumedCount += valueConsumed;
            }
        }

        return consumedCount;
    }
}
