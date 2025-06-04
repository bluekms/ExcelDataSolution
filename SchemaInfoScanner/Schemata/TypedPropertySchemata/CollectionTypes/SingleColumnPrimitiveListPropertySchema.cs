using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;

namespace SchemaInfoScanner.Schemata.TypedPropertySchemata.CollectionTypes;

public sealed record SingleColumnPrimitiveListPropertySchema(
    PrimitiveTypeGenericArgumentSchema GenericArgumentSchema,
    INamedTypeSymbol NamedTypeSymbol,
    IReadOnlyList<AttributeSyntax> AttributeList,
    string Separator)
    : PropertySchemaBase(GenericArgumentSchema.PropertyName, NamedTypeSymbol, AttributeList)
{
    protected override int OnCheckCompatibility(CompatibilityContext context, ILogger logger)
    {
        var subContext = CompatibilityContext.CreateSingleColumnCollectionContext(context, Separator);

        var consumed = 0;
        while (subContext.StartIndex + consumed < subContext.Arguments.Count)
        {
            var nestedContext = subContext with { StartIndex = subContext.StartIndex + consumed };
            consumed += GenericArgumentSchema.CheckCompatibility(nestedContext, logger);
        }

        return 1;
    }
}
