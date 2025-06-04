using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;

namespace SchemaInfoScanner.Schemata.TypedPropertySchemata.RecordTypes;

public sealed record RecordKeyRecordValueDictionarySchema(
    RecordTypeGenericArgumentSchema KeyGenericArgumentSchema,
    RecordTypeGenericArgumentSchema ValueGenericArgumentSchema,
    INamedTypeSymbol NamedTypeSymbol,
    IReadOnlyList<AttributeSyntax> AttributeList)
    : PropertySchemaBase(KeyGenericArgumentSchema.PropertyName, NamedTypeSymbol, AttributeList)
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

            consumedCount += KeyGenericArgumentSchema.CheckCompatibility(keyContext, logger);

            var valueContext = CompatibilityContext.CreateContext(
                context.Arguments,
                context.StartIndex + consumedCount,
                context.EnumMemberCatalog);

            consumedCount += ValueGenericArgumentSchema.CheckCompatibility(valueContext, logger);
        }

        return consumedCount;
    }
}
