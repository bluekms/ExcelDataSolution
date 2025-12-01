using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using StaticDataAttribute;

namespace SchemaInfoScanner.Schemata.TypedPropertySchemata.CollectionTypes;

public sealed record SingleColumnPrimitiveSetPropertySchema(
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

        var nestedContext = CompatibilityContext.CreateCollectAll(context.EnumMemberCatalog, arguments);
        for (var i = 0; i < arguments.Length; i++)
        {
            GenericArgumentSchema.CheckCompatibility(nestedContext);
        }

        nestedContext.ValidateNoDuplicates();
    }
}
