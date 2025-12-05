using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.NameObjects;
namespace SchemaInfoScanner.Schemata.TypedPropertySchemata.PrimitiveTypes;

public sealed record EnumPropertySchema(
    PropertyName PropertyName,
    INamedTypeSymbol NamedTypeSymbol,
    IReadOnlyList<AttributeSyntax> AttributeList)
    : PropertySchemaBase(PropertyName, NamedTypeSymbol, AttributeList)
{
    protected override void OnCheckCompatibility(CompatibilityContext context)
    {
        var enumName = new EnumName(NamedTypeSymbol);
        var enumMembers = context.MetadataCatalogs.EnumMemberCatalog.GetEnumMembers(enumName);

        var cell = context.Consume();
        var value = cell.Value;

        if (!enumMembers.Contains(value))
        {
            throw new InvalidOperationException(
                $"Invalid value '{cell.Value}' in cell {cell.Address}. Expected a member of {enumName.FullName}.");
        }

        context.Collect(value);
    }
}
