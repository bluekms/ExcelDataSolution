using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.NameObjects;

namespace SchemaInfoScanner.Schemata.TypedParameterSchemata;

public sealed record EnumParameterSchema(
    ParameterName ParameterName,
    INamedTypeSymbol NamedTypeSymbol,
    IReadOnlyList<AttributeSyntax> AttributeList)
    : ParameterSchemaBase(ParameterName, NamedTypeSymbol, AttributeList)
{
    protected override void OnCheckCompatibility(string argument, ILogger logger)
    {
        throw new InvalidOperationException($"{ParameterName.FullName} is enum. Use CheckCompatibility(string, EnumMemberContainer) instead.");
    }

    public void CheckCompatibility(string argument, EnumMemberContainer enumMemberContainer)
    {
        var enumName = new EnumName(NamedTypeSymbol.Name);
        var enumMembers = enumMemberContainer.GetEnumMembers(enumName);
        if (!enumMembers.Contains(argument))
        {
            throw new InvalidOperationException($"{argument} is not a member of {enumName.FullName}.");
        }
    }
}
