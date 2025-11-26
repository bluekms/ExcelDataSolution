using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.NameObjects;
using StaticDataAttribute;

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
        if (!TryGetAttributeValue<LengthAttribute, int>(out var length))
        {
            throw new InvalidOperationException($"Parameter {PropertyName} cannot have LengthAttribute in the argument: {context}");
        }

        if (context.Arguments.Count % 2 != 0)
        {
            throw new InvalidOperationException($"Invalid data length: {context}");
        }

        var isKey = true;
        var keys = new List<string>();
        for (var i = 0; i < context.Arguments.Count; i++)
        {
            if (isKey)
            {
                isKey = false;
                keys.Add(context.Consume());

                KeySchema.CheckCompatibility(context);
            }
            else
            {
                isKey = true;
                ValueSchema.CheckCompatibility(context);
            }
        }

        var keysList = context.GetCollectedValues()
            .Where((x, i) => i % 2 == 0)
            .ToList();

        var keyCount = keysList.ToHashSet().Count;

        if (keys.Count != keyCount)
        {
            throw new InvalidOperationException($"Parameter {PropertyName} has mismatched length between keys and values in context {context}.");
        }
    }
}
