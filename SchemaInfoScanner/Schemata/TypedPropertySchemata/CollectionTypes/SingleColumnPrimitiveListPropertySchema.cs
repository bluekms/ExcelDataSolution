using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner.Catalogs;

namespace SchemaInfoScanner.Schemata.TypedPropertySchemata.CollectionTypes;

public sealed record SingleColumnPrimitiveListPropertySchema(
    PrimitiveTypeGenericArgumentSchema GenericArgumentSchema,
    INamedTypeSymbol NamedTypeSymbol,
    IReadOnlyList<AttributeSyntax> AttributeList,
    string Separator)
    : PropertySchemaBase(GenericArgumentSchema.PropertyName, NamedTypeSymbol, AttributeList)
{
    protected override void OnCheckCompatibility(
        IEnumerator<string> arguments,
        EnumMemberCatalog enumMemberCatalog,
        ILogger logger)
    {
        var argument = GetNextArgument(arguments, GetType(), logger);
        var split = argument
            .Split(Separator)
            .Select(x => x.Trim());

        foreach (var item in split)
        {
            GenericArgumentSchema.NestedSchema.CheckCompatibility(item, enumMemberCatalog, logger);
        }
    }
}
