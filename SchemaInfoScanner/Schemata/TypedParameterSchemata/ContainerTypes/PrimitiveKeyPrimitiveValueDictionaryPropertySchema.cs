using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.NameObjects;

namespace SchemaInfoScanner.Schemata.TypedParameterSchemata.ContainerTypes;

public sealed record PrimitiveKeyPrimitiveValueDictionaryPropertySchema(
    ParameterName ParameterName,
    INamedTypeSymbol NamedTypeSymbol,
    IReadOnlyList<AttributeSyntax> AttributeList,
    PrimitiveTypeGenericArgumentSchema KeySchema,
    PrimitiveTypeGenericArgumentSchema ValueSchema)
    : PropertySchemaBase(ParameterName, NamedTypeSymbol, AttributeList)
{
    protected override void OnCheckCompatibility(
        IEnumerator<string> arguments,
        EnumMemberContainer enumMemberContainer,
        ILogger logger)
    {
        var keyArgument = GetNextArgument(arguments, GetType(), logger);
        KeySchema.InnerSchema.CheckCompatibility(keyArgument, enumMemberContainer, logger);

        var valueArgument = GetNextArgument(arguments, GetType(), logger);
        ValueSchema.InnerSchema.CheckCompatibility(valueArgument, enumMemberContainer, logger);
    }
}
