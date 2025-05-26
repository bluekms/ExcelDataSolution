using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner.Containers;

namespace SchemaInfoScanner.Schemata.TypedPropertySchemata.RecordTypes;

public sealed record RecordKeyRecordValueDictionarySchema(
    RecordTypeGenericArgumentSchema KeyGenericArgumentSchema,
    RecordTypeGenericArgumentSchema ValueGenericArgumentSchema,
    INamedTypeSymbol NamedTypeSymbol,
    IReadOnlyList<AttributeSyntax> AttributeList)
    : PropertySchemaBase(KeyGenericArgumentSchema.PropertyName, NamedTypeSymbol, AttributeList)
{
    protected override void OnCheckCompatibility(
        IEnumerator<string> arguments,
        EnumMemberContainer enumMemberContainer,
        ILogger logger)
    {
        KeyGenericArgumentSchema.NestedSchema.CheckCompatibility(arguments, enumMemberContainer, logger);
        ValueGenericArgumentSchema.NestedSchema.CheckCompatibility(arguments, enumMemberContainer, logger);
    }
}
