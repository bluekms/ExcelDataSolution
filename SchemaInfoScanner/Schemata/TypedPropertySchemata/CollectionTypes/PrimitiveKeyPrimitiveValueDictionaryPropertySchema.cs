using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.NameObjects;

namespace SchemaInfoScanner.Schemata.TypedPropertySchemata.CollectionTypes;

public sealed record PrimitiveKeyPrimitiveValueDictionaryPropertySchema(
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

        if (context.CollectionLength % 2 != 0)
        {
            throw new InvalidOperationException($"Invalid data length: {context}");
        }

        var isKey = true;
        var consumedCount = 0;
        for (var i = 0; i < context.CollectionLength; i++)
        {
            if (isKey)
            {
                var keyConsumed = KeySchema.CheckCompatibility(context);
                context.StartIndex += keyConsumed;
                consumedCount += keyConsumed;
                isKey = false;
            }
            else
            {
                var valueConsumed = ValueSchema.CheckCompatibility(context);
                context.StartIndex += valueConsumed;
                consumedCount += valueConsumed;
                isKey = true;
            }
        }

        return consumedCount;
    }
}
