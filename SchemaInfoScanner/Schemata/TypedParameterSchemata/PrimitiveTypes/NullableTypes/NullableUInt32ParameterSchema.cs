using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.Schemata.AttributeCheckers;

namespace SchemaInfoScanner.Schemata.TypedParameterSchemata.PrimitiveTypes.NullableTypes;

public sealed record NullableUInt32ParameterSchema(
    ParameterName ParameterName,
    INamedTypeSymbol NamedTypeSymbol,
    IReadOnlyList<AttributeSyntax> AttributeList)
    : ParameterSchemaBase(ParameterName, NamedTypeSymbol, AttributeList)
{
    protected override void OnCheckCompatibility(
        IEnumerator<string> arguments,
        EnumMemberContainer enumMemberContainer,
        ILogger logger)
    {
        var argument = GetNextArgument(arguments, GetType(), logger);
        var result = NullStringAttributeChecker.Check(this, argument);
        if (result.IsNull)
        {
            return;
        }

        var schema = new UInt32ParameterSchema(ParameterName, NamedTypeSymbol, AttributeList);
        schema.CheckCompatibility(arguments, enumMemberContainer, logger);
    }
}
