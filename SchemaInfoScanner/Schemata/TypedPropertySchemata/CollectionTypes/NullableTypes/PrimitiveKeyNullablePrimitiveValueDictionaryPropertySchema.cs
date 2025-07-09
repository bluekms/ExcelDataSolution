using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.Schemata.AttributeCheckers;
using SchemaInfoScanner.Schemata.CompatibilityContexts;

namespace SchemaInfoScanner.Schemata.TypedPropertySchemata.CollectionTypes.NullableTypes;

public sealed record PrimitiveKeyNullablePrimitiveValueDictionaryPropertySchema(
    PropertyName PropertyName,
    INamedTypeSymbol NamedTypeSymbol,
    IReadOnlyList<AttributeSyntax> AttributeList,
    PrimitiveTypeGenericArgumentSchema KeySchema,
    PrimitiveTypeGenericArgumentSchema ValueSchema)
    : PropertySchemaBase(PropertyName, NamedTypeSymbol, AttributeList)
{
    protected override int OnCheckCompatibility(ICompatibilityContext context, ILogger logger)
    {
        if (!context.IsCollection)
        {
            throw new InvalidOperationException($"Invalid context: {context}");
        }

        var consumedCount = 0;
        for (var i = 0; i < context.CollectionLength; i++)
        {
            var keyContext = context.WithStartIndex(context.StartIndex + consumedCount);
            consumedCount += KeySchema.CheckCompatibility(keyContext, logger);

            var valueContext = context.WithStartIndex(context.StartIndex + consumedCount);
            var result = NullStringAttributeChecker.Check(this, valueContext.CurrentArgument);
            if (!result.IsNull)
            {
                consumedCount += ValueSchema.CheckCompatibility(valueContext, logger);
            }
        }

        return consumedCount;
    }
}
