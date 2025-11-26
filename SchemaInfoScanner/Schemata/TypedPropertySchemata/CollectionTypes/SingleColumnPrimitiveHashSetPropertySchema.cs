using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SchemaInfoScanner.Schemata.TypedPropertySchemata.CollectionTypes;

public sealed record SingleColumnPrimitiveHashSetPropertySchema(
    PrimitiveTypeGenericArgumentSchema GenericArgumentSchema,
    INamedTypeSymbol NamedTypeSymbol,
    IReadOnlyList<AttributeSyntax> AttributeList,
    string Separator)
    : PropertySchemaBase(GenericArgumentSchema.PropertyName, NamedTypeSymbol, AttributeList)
{
    protected override void OnCheckCompatibility(CompatibilityContext context)
    {
        var arguments = context.Consume().Split(Separator);

        var values = new List<object?>(arguments.Length);
        foreach (var argument in arguments)
        {
            if (string.IsNullOrWhiteSpace(argument))
            {
                throw new InvalidOperationException($"Parameter {PropertyName} has empty value in the argument: {context}");
            }

            var nestedContext = new CompatibilityContext(context.EnumMemberCatalog, [argument]);
            GenericArgumentSchema.CheckCompatibility(nestedContext);

            values.Add(nestedContext.GetCollectedValues()[^1]);
        }

        var hs = new HashSet<object?>();
        foreach (var value in values)
        {
            if (!hs.Add(value))
            {
                throw new InvalidOperationException($"Parameter {PropertyName} has duplicate value in the argument: {context}");
            }
        }
    }
}
