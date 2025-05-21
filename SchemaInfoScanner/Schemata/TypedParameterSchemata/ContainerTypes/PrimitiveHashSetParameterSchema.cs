using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.NameObjects;

namespace SchemaInfoScanner.Schemata.TypedParameterSchemata.ContainerTypes;

public sealed record PrimitiveHashSetParameterSchema(
    PrimitiveTypeGenericArgumentSchema GenericArgumentSchema,
    INamedTypeSymbol NamedTypeSymbol,
    IReadOnlyList<AttributeSyntax> AttributeList)
    : ParameterSchemaBase(GenericArgumentSchema.ParameterName, NamedTypeSymbol, AttributeList)
{
    protected override void OnCheckCompatibility(
        IEnumerator<string> arguments,
        EnumMemberContainer enumMemberContainer,
        ILogger logger)
    {
        GenericArgumentSchema.InnerSchema.CheckCompatibility(arguments, enumMemberContainer, logger);
    }
}
