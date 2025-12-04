using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.Schemata.AttributeCheckers;
using StaticDataAttribute;

namespace SchemaInfoScanner.Schemata.TypedPropertySchemata.CollectionTypes.NullableTypes;

public sealed record SingleColumnNullablePrimitiveSetPropertySchema(
    PrimitiveTypeGenericArgumentSchema GenericArgumentSchema,
    INamedTypeSymbol NamedTypeSymbol,
    IReadOnlyList<AttributeSyntax> AttributeList,
    string Separator)
    : PropertySchemaBase(GenericArgumentSchema.PropertyName, NamedTypeSymbol, AttributeList)
{
    protected override void OnCheckCompatibility(CompatibilityContext context)
    {
        var cell = context.Consume();
        var arguments = cell.Value.Split(Separator);

        if (TryGetAttributeValue<LengthAttribute, int>(out var length))
        {
            if (arguments.Length != length)
            {
                throw new InvalidOperationException(
                    $"Cell {cell.Address} contains {arguments.Length} value(s), but {PropertyName} expects {length}.");
            }
        }

        foreach (var argument in arguments)
        {
            var result = NullStringAttributeChecker.Check(this, argument);
            if (!result.IsNull)
            {
                var nestedCells = new[] { new CellData(cell.Address, argument) };
                var nestedContext = CompatibilityContext.CreateCollectAll(
                    context.EnumMemberCatalog,
                    nestedCells);

                GenericArgumentSchema.CheckCompatibility(nestedContext);
            }
        }

        context.ValidateNoDuplicates();
    }
}
