using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner.Schemata.CompatibilityContexts;

namespace SchemaInfoScanner.Schemata.TypedPropertySchemata.RecordTypes;

public sealed record RecordListPropertySchema(
    RecordTypeGenericArgumentSchema GenericArgumentSchema,
    INamedTypeSymbol NamedTypeSymbol,
    IReadOnlyList<AttributeSyntax> AttributeList)
    : PropertySchemaBase(GenericArgumentSchema.PropertyName, NamedTypeSymbol, AttributeList)
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
            var nestedContext = new CompatibilityContext(
                context.EnumMemberCatalog,
                context.Arguments,
                context.StartIndex + consumedCount,
                context.CollectionLength);

            consumedCount += GenericArgumentSchema.CheckCompatibility(nestedContext, logger);
        }

        return consumedCount;
    }
}
