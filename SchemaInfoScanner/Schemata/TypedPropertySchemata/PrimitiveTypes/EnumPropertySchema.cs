using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.Schemata.CompatibilityContexts;

namespace SchemaInfoScanner.Schemata.TypedPropertySchemata.PrimitiveTypes;

public sealed record EnumPropertySchema(
    PropertyName PropertyName,
    INamedTypeSymbol NamedTypeSymbol,
    IReadOnlyList<AttributeSyntax> AttributeList)
    : PropertySchemaBase(PropertyName, NamedTypeSymbol, AttributeList)
{
    protected override int OnCheckCompatibility(ICompatibilityContext context)
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
