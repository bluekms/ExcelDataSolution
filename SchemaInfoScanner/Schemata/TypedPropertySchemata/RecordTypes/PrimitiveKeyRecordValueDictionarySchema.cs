using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.Schemata.TypedPropertySchemata.CollectionTypes;
using StaticDataAttribute;

namespace SchemaInfoScanner.Schemata.TypedPropertySchemata.RecordTypes;

public sealed record PrimitiveKeyRecordValueDictionarySchema(
    PrimitiveTypeGenericArgumentSchema KeySchema,
    RecordTypeGenericArgumentSchema ValueSchema,
    INamedTypeSymbol NamedTypeSymbol,
    IReadOnlyList<AttributeSyntax> AttributeList)
    : PropertySchemaBase(KeySchema.PropertyName, NamedTypeSymbol, AttributeList)
{
    protected override void OnCheckCompatibility(CompatibilityContext context)
    {
        if (!TryGetAttributeValue<LengthAttribute, int>(out var length))
        {
            throw new InvalidOperationException($"Parameter {PropertyName} cannot have LengthAttribute in the argument: {context}");
        }

        var keys = new List<object?>();
        for (var i = 0; i < length; i++)
        {
            KeySchema.CheckCompatibility(context);
            keys.Add(context.GetCollectedValues()[^1]);

            ValueSchema.CheckCompatibility(context);
        }

        var hs = new HashSet<object?>();
        foreach (var key in keys)
        {
            if (!hs.Add(key))
            {
                throw new InvalidOperationException($"Parameter {PropertyName} has duplicate key: {key} in context {context}.");
            }
        }
    }
}
