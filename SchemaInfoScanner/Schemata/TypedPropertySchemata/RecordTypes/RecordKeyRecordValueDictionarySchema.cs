using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SchemaInfoScanner.Schemata.TypedPropertySchemata.RecordTypes;

public sealed record RecordKeyRecordValueDictionarySchema(
    RecordTypeGenericArgumentSchema KeyGenericArgumentSchema,
    RecordTypeGenericArgumentSchema ValueGenericArgumentSchema,
    INamedTypeSymbol NamedTypeSymbol,
    IReadOnlyList<AttributeSyntax> AttributeList)
    : PropertySchemaBase(KeyGenericArgumentSchema.PropertyName, NamedTypeSymbol, AttributeList)
{
    protected override int OnCheckCompatibility(CompatibilityContext context)
    {
        if (!context.IsCollection)
        {
            throw new InvalidOperationException($"Invalid context: {context}");
        }

        var consumedCount = 0;
        for (var i = 0; i < context.CollectionLength; i++)
        {
            var keyContext = new CompatibilityContext(
                context.EnumMemberCatalog,
                context.Arguments,
                context.StartIndex + consumedCount);

            consumedCount += KeyGenericArgumentSchema.CheckCompatibility(keyContext);

            var valueContext = new CompatibilityContext(
                context.EnumMemberCatalog,
                context.Arguments,
                context.StartIndex + consumedCount);

            consumedCount += ValueGenericArgumentSchema.CheckCompatibility(valueContext);
        }

        return consumedCount;
    }
}
