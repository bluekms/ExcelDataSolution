using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.Schemata.AttributeCheckers;

namespace SchemaInfoScanner.Schemata.TypedPropertySchemata.CollectionTypes.NullableTypes;

public sealed record SingleColumnNullablePrimitiveHashSetPropertySchema(
    PrimitiveTypeGenericArgumentSchema GenericArgumentSchema,
    INamedTypeSymbol NamedTypeSymbol,
    IReadOnlyList<AttributeSyntax> AttributeList,
    string Separator)
    : PropertySchemaBase(GenericArgumentSchema.PropertyName, NamedTypeSymbol, AttributeList)
{
    protected override int OnCheckCompatibility(CompatibilityContext context)
    {
        var arguments = context.CurrentArgument.Split(Separator);

        var hashSet = arguments.ToHashSet();
        if (hashSet.Count != arguments.Length)
        {
            throw new InvalidOperationException($"Parameter {PropertyName} has duplicate values in the argument: {context}");
        }

        foreach (var argument in arguments)
        {
            var result = NullStringAttributeChecker.Check(this, argument);
            if (!result.IsNull)
            {
                var nestedContext = new CompatibilityContext(context.EnumMemberCatalog, [argument]);
                GenericArgumentSchema.CheckCompatibility(nestedContext);
            }
        }

        return 1;
    }
}
