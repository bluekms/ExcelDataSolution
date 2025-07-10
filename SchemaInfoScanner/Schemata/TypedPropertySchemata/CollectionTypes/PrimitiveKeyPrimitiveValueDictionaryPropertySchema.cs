using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.Schemata.CompatibilityContexts;

namespace SchemaInfoScanner.Schemata.TypedPropertySchemata.CollectionTypes;

public sealed record PrimitiveKeyPrimitiveValueDictionaryPropertySchema(
    PropertyName PropertyName,
    INamedTypeSymbol NamedTypeSymbol,
    IReadOnlyList<AttributeSyntax> AttributeList,
    PrimitiveTypeGenericArgumentSchema KeySchema,
    PrimitiveTypeGenericArgumentSchema ValueSchema)
    : PropertySchemaBase(PropertyName, NamedTypeSymbol, AttributeList)
{
    protected override int OnCheckCompatibility(ICompatibilityContext context)
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
            var contextAtIndex = context.WithStartIndex(context.StartIndex + i);
            if (isKey)
            {
                consumedCount += KeySchema.CheckCompatibility(contextAtIndex);
                isKey = false;
            }
            else
            {
                consumedCount += ValueSchema.CheckCompatibility(contextAtIndex);
                isKey = true;
            }
        }

        return consumedCount;
    }
}
