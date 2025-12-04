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
        var enumName = new EnumName(NamedTypeSymbol.Name);
        var enumMembers = context.EnumMemberCatalog.GetEnumMembers(enumName);

        var cell = context.Consume();
        var value = cell.Value;

        if (!enumMembers.Contains(value))
        {
            throw new InvalidOperationException(
                $"The value '{value}' in cell {cell.Address} is not a valid member of {enumName.FullName}.");
        }

        context.Collect(value);
    }
}
