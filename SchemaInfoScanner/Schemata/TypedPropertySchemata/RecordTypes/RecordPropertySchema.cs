using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner.Catalogs;
using SchemaInfoScanner.NameObjects;

namespace SchemaInfoScanner.Schemata.TypedPropertySchemata.RecordTypes;

public sealed record RecordPropertySchema(
    PropertyName PropertyName,
    INamedTypeSymbol NamedTypeSymbol,
    IReadOnlyList<AttributeSyntax> AttributeList,
    IReadOnlyList<PropertySchemaBase> MemberSchemata)
    : PropertySchemaBase(PropertyName, NamedTypeSymbol, AttributeList)
{
    protected override void OnCheckCompatibility(
        IEnumerator<string> arguments,
        EnumMemberCatalog enumMemberCatalog,
        ILogger logger)
    {
        foreach (var schema in MemberSchemata)
        {
            schema.CheckCompatibility(arguments, enumMemberCatalog, logger);
        }
    }
}
