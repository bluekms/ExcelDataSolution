using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using StaticDataAttribute;

namespace SchemaInfoScanner.Schemata.TypedPropertySchemata.CollectionTypes;

public sealed record SingleColumnPrimitiveArrayPropertySchema(
    PrimitiveTypeGenericArgumentSchema GenericArgumentSchema,
    INamedTypeSymbol NamedTypeSymbol,
    IReadOnlyList<AttributeSyntax> AttributeList,
    string Separator)
    : PropertySchemaBase(GenericArgumentSchema.PropertyName, NamedTypeSymbol, AttributeList)
{
    protected override void OnCheckCompatibility(CompatibilityContext context)
    {
        var arguments = context.Consume().Split(Separator);

        if (TryGetAttributeValue<LengthAttribute, int>(out var length))
        {
            if (arguments.Length != length)
            {
                throw new InvalidOperationException($"{PropertyName} has {nameof(LengthAttribute)} ({length}). but context length ({arguments.Length}).");
            }
        }

        foreach (var argument in arguments)
        {
            if (string.IsNullOrWhiteSpace(argument))
            {
                throw new InvalidOperationException($"Parameter {PropertyName} has empty value in the argument: {context}");
            }

            var nestedContext = CompatibilityContext.CreateNoCollect(context.EnumMemberCatalog, [argument]);
            GenericArgumentSchema.CheckCompatibility(nestedContext);
        }
    }
}
