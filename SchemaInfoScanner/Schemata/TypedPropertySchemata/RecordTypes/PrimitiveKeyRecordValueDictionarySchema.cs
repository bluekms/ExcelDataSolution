using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.Schemata.CompatibilityContexts;
using SchemaInfoScanner.Schemata.TypedPropertySchemata.CollectionTypes;

namespace SchemaInfoScanner.Schemata.TypedPropertySchemata.RecordTypes;

public sealed record PrimitiveKeyRecordValueDictionarySchema(
    PrimitiveTypeGenericArgumentSchema KeySchema,
    RecordTypeGenericArgumentSchema ValueSchema,
    INamedTypeSymbol NamedTypeSymbol,
    IReadOnlyList<AttributeSyntax> AttributeList)
    : PropertySchemaBase(KeySchema.PropertyName, NamedTypeSymbol, AttributeList)
{
    protected override int OnCheckCompatibility(ICompatibilityContext context)
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

            consumedCount += KeySchema.CheckCompatibility(keyContext);

            var valueContext = new CompatibilityContext(
                context.EnumMemberCatalog,
                context.Arguments,
                context.StartIndex + consumedCount);

            consumedCount += ValueSchema.CheckCompatibility(valueContext);
        }

        return consumedCount;
    }
}
