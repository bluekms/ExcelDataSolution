using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.Schemata.AttributeCheckers;

namespace SchemaInfoScanner.Schemata.TypedPropertySchemata.CollectionTypes.NullableTypes;

public sealed record NullablePrimitiveHashSetPropertySchema(
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
        var values = new List<object?>();
        for (var i = 0; i < context.CollectionLength; i++)
        {
            var result = NullStringAttributeChecker.Check(this, context.CurrentArgument);
            if (result.IsNull)
            {
                context.Collect(null);
                context.StartIndex += 1;
                totalConsumed += 1;
            }
            else
            {
                var consumed = GenericArgumentSchema.CheckCompatibility(context);
                context.StartIndex += consumed;
                totalConsumed += consumed;
            }

            values.Add(context.GetCollectedValues()[^1]);
        }

        var hs = new HashSet<object?>();
        foreach (var value in values)
        {
            if (!hs.Add(value))
            {
                throw new InvalidOperationException($"Parameter {PropertyName} has duplicate value in the argument: {context}");
            }
        }

        return totalConsumed;
    }
}
