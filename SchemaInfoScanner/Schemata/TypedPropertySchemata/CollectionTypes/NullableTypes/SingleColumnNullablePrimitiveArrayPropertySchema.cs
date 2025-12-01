using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.Schemata.AttributeCheckers;
using StaticDataAttribute;

namespace SchemaInfoScanner.Schemata.TypedPropertySchemata.CollectionTypes.NullableTypes;

public sealed record SingleColumnNullablePrimitiveArrayPropertySchema(
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
            var result = NullStringAttributeChecker.Check(this, argument);
            if (result.IsNull)
            {
                context.ConsumeNull();
            }
            else
            {
                var nestedContext = CompatibilityContext.CreateNoCollect(context.EnumMemberCatalog, [argument]);
                GenericArgumentSchema.CheckCompatibility(nestedContext);
            }
        }
    }
}
