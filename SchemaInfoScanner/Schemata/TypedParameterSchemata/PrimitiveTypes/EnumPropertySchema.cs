using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.NameObjects;

namespace SchemaInfoScanner.Schemata.TypedParameterSchemata.PrimitiveTypes;

public sealed record EnumPropertySchema(
    PropertyName PropertyName,
    INamedTypeSymbol NamedTypeSymbol,
    IReadOnlyList<AttributeSyntax> AttributeList)
    : PropertySchemaBase(PropertyName, NamedTypeSymbol, AttributeList)
{
    protected override void OnCheckCompatibility(
        IEnumerator<string> arguments,
        EnumMemberContainer enumMemberContainer,
        ILogger logger)
    {
        var enumName = new EnumName(NamedTypeSymbol.Name);
        var enumMembers = enumMemberContainer.GetEnumMembers(enumName);

        var argument = GetNextArgument(arguments, GetType(), logger);
        if (!enumMembers.Contains(argument))
        {
            throw new InvalidOperationException($"{arguments} is not a member of {enumName.FullName}");
        }
    }
}
