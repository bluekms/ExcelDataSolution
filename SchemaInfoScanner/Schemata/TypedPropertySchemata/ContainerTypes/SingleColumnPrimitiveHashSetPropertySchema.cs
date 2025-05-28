using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.NameObjects;

namespace SchemaInfoScanner.Schemata.TypedPropertySchemata.ContainerTypes;

public sealed record SingleColumnPrimitiveHashSetPropertySchema(
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
            .Select(x => x.Trim())
            .ToList();

        foreach (var item in split)
        {
            GenericArgumentSchema.NestedSchema.CheckCompatibility(item, enumMemberCatalog, logger);
        }

        var hashSet = split.ToHashSet();
        if (split.Count != hashSet.Count)
        {
            var ex = new InvalidOperationException(
                $"Parameter {PropertyName} has duplicate values in the argument: {arguments}");
            LogError(logger, GetType(), argument, ex, ex.InnerException);
            throw ex;
        }
    }
}
