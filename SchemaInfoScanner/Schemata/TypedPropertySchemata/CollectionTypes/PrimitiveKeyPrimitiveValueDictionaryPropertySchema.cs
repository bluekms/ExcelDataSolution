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
    protected override void OnCheckCompatibility(CompatibilityContext context)
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
        for (var i = 0; i < context.CollectionLength; i++)
        {
            if (isKey)
            {
                KeySchema.CheckCompatibility(context);
                isKey = false;
            }
            else
            {
                ValueSchema.CheckCompatibility(context);
                isKey = true;
            }
        }
    }
}
