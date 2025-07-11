using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SchemaInfoScanner.Schemata.TypedPropertySchemata.CollectionTypes;

public sealed record PrimitiveHashSetPropertySchema(
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

        var values = new List<object?>();
        for (var i = 0; i < context.CollectionLength; i++)
        {
            GenericArgumentSchema.CheckCompatibility(context);
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
    }
}
