using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.Schemata.AttributeCheckers;

namespace SchemaInfoScanner.Schemata.TypedPropertySchemata.PrimitiveTypes.NullableTypes;

public sealed record NullableBytePropertySchema(
    PropertyName PropertyName,
    INamedTypeSymbol NamedTypeSymbol,
    IReadOnlyList<AttributeSyntax> AttributeList)
    : PropertySchemaBase(PropertyName, NamedTypeSymbol, AttributeList)
{
    protected override void OnCheckCompatibility(
        IEnumerator<string> arguments,
        EnumMemberCatalog enumMemberCatalog,
        ILogger logger)
    {
        var argument = GetNextArgument(arguments, GetType(), logger);
        var result = NullStringAttributeChecker.Check(this, argument);
        if (result.IsNull)
        {
            return;
        }

        var schema = new BytePropertySchema(PropertyName, NamedTypeSymbol, AttributeList);
        schema.CheckCompatibility(arguments, enumMemberCatalog, logger);
    }
}
