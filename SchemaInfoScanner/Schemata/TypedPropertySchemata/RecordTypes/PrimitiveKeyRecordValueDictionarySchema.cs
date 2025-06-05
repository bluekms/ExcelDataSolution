using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner.Schemata.TypedPropertySchemata.CollectionTypes;

namespace SchemaInfoScanner.Schemata.TypedPropertySchemata.RecordTypes;

public sealed record PrimitiveKeyRecordValueDictionarySchema(
    PrimitiveTypeGenericArgumentSchema KeySchema,
    RecordTypeGenericArgumentSchema ValueSchema,
    INamedTypeSymbol NamedTypeSymbol,
    IReadOnlyList<AttributeSyntax> AttributeList)
    : PropertySchemaBase(KeySchema.PropertyName, NamedTypeSymbol, AttributeList)
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
