using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner.Catalogs;
using SchemaInfoScanner.NameObjects;

namespace SchemaInfoScanner.Schemata.TypedPropertySchemata.CollectionTypes;

public sealed record PrimitiveKeyPrimitiveValueDictionaryPropertySchema(
    PropertyName PropertyName,
    INamedTypeSymbol NamedTypeSymbol,
    IReadOnlyList<AttributeSyntax> AttributeList,
    PrimitiveTypeGenericArgumentSchema KeySchema,
    PrimitiveTypeGenericArgumentSchema ValueSchema)
    : PropertySchemaBase(PropertyName, NamedTypeSymbol, AttributeList)
{
    protected override void OnCheckCompatibility(
        IEnumerator<string> arguments,
        EnumMemberCatalog enumMemberCatalog,
        ILogger logger)
    {
        var keyArgument = GetNextArgument(arguments, GetType(), logger);
        KeySchema.NestedSchema.CheckCompatibility(keyArgument, enumMemberCatalog, logger);

        var valueArgument = GetNextArgument(arguments, GetType(), logger);
        ValueSchema.NestedSchema.CheckCompatibility(valueArgument, enumMemberCatalog, logger);
    }
}
