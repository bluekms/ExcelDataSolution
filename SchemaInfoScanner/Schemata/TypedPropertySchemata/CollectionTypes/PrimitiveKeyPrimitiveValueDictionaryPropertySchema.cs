using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
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
    protected override int OnCheckCompatibility(CompatibilityContext context, ILogger logger)
    {
        if (!context.IsCollection)
        {
            throw new InvalidOperationException($"Invalid context: {context}");
        }

        var consumedCount = 0;
        for (var i = 0; i < context.CollectionLength; i++)
        {
            var keyContext = CompatibilityContext.CreateContext(
                context.Arguments,
                context.StartIndex + consumedCount,
                context.EnumMemberCatalog);

            consumedCount += KeySchema.CheckCompatibility(keyContext, logger);

            var valueContext = CompatibilityContext.CreateContext(
                context.Arguments,
                context.StartIndex + consumedCount,
                context.EnumMemberCatalog);

            consumedCount += ValueSchema.CheckCompatibility(valueContext, logger);
        }

        return consumedCount;
    }
}
