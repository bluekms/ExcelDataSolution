using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.NameObjects;

namespace SchemaInfoScanner.Schemata.TypedParameterSchemata;

public sealed record SingleColumnContainerParameterSchema(
    ParameterName ParameterName,
    INamedTypeSymbol NamedTypeSymbol,
    IReadOnlyList<AttributeSyntax> AttributeList,
    string Separator,
    ParameterSchemaBase InnerParameterSchema)
    : ParameterSchemaBase(ParameterName, NamedTypeSymbol, AttributeList)
{
    public override void CheckCompatibility(string argument)
    {
        var split = argument
            .Split(Separator)
            .Select(x => x.Trim());

        foreach (var item in split)
        {
            InnerParameterSchema.CheckCompatibility(item);
        }
    }
}
