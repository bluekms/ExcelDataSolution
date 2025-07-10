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
    protected override int OnCheckCompatibility(CompatibilityContext context)
    {
        var enumName = new EnumName(NamedTypeSymbol.Name);
        var enumMembers = context.EnumMemberCatalog.GetEnumMembers(enumName);

        var argument = context.CurrentArgument;
        if (!enumMembers.Contains(argument))
        {
            throw new InvalidOperationException($"{context} is not a member of {enumName.FullName}");
        }

        return 1;
    }
}
