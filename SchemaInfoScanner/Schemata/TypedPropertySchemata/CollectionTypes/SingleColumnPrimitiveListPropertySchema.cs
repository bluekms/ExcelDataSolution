using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SchemaInfoScanner.Schemata.TypedPropertySchemata.CollectionTypes;

public sealed record SingleColumnPrimitiveListPropertySchema(
    PrimitiveTypeGenericArgumentSchema GenericArgumentSchema,
    INamedTypeSymbol NamedTypeSymbol,
    IReadOnlyList<AttributeSyntax> AttributeList,
    string Separator)
    : PropertySchemaBase(GenericArgumentSchema.PropertyName, NamedTypeSymbol, AttributeList)
{
    protected override int OnCheckCompatibility(CompatibilityContext context)
    {
        var arguments = context.CurrentArgument.Split(Separator);

        foreach (var argument in arguments)
        {
            if (string.IsNullOrWhiteSpace(argument))
            {
                throw new InvalidOperationException($"Parameter {PropertyName} has empty value in the argument: {context}");
            }

            var nestedContext = new CompatibilityContext(context.EnumMemberCatalog, [argument]);
            GenericArgumentSchema.CheckCompatibility(nestedContext);
        }

        return 1;
    }
}
