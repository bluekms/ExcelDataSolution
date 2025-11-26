using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using StaticDataAttribute;

namespace SchemaInfoScanner.Schemata.TypedPropertySchemata.RecordTypes;

public sealed record RecordKeyRecordValueDictionarySchema(
    RecordTypeGenericArgumentSchema KeyGenericArgumentSchema,
    RecordTypeGenericArgumentSchema ValueGenericArgumentSchema,
    INamedTypeSymbol NamedTypeSymbol,
    IReadOnlyList<AttributeSyntax> AttributeList)
    : PropertySchemaBase(KeyGenericArgumentSchema.PropertyName, NamedTypeSymbol, AttributeList)
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
            KeyGenericArgumentSchema.CheckCompatibility(context);
            keys.Add(context.GetCollectedValues()[^1]);

            ValueGenericArgumentSchema.CheckCompatibility(context);
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
