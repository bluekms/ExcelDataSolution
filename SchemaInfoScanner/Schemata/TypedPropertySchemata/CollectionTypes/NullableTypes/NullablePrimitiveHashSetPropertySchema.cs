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
        var arguments = new List<string>();
        for (var i = 0; i < context.CollectionLength; i++)
        {
            var nestedContext = context.WithStartIndex(context.StartIndex + totalConsumed);
            arguments.Add(nestedContext.CurrentArgument);

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

        var hasDuplicates = arguments
            .GroupBy(x => x)
            .Any(x => x.Count() > 1);

        if (hasDuplicates)
        {
            throw new InvalidOperationException($"Parameter {PropertyName} has duplicate values in the argument: {context}");
        }

        return totalConsumed;
    }
}
