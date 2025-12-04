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
        var cell = context.Consume();
        var parts = cell.Value.Split(Separator);

        if (TryGetAttributeValue<LengthAttribute, int>(out var length))
        {
            if (parts.Length != length)
            {
                throw new InvalidOperationException(
                    $"Cell {cell.Address} contains {parts.Length} value(s), but {PropertyName} expects {length}.");
            }
        }

        var nestedCells = parts
            .Select(x => new CellData(cell.Address, x))
            .ToArray();

        var nestedContext = CompatibilityContext.CreateCollectAll(context.EnumMemberCatalog, nestedCells);
        for (var i = 0; i < parts.Length; i++)
        {
            GenericArgumentSchema.CheckCompatibility(nestedContext);
        }

        nestedContext.ValidateNoDuplicates();
    }
}
