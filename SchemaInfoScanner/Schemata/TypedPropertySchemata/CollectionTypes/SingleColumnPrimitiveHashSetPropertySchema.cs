using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner.Schemata.CompatibilityContexts;

namespace SchemaInfoScanner.Schemata.TypedPropertySchemata.CollectionTypes;

public sealed record SingleColumnPrimitiveHashSetPropertySchema(
    PrimitiveTypeGenericArgumentSchema GenericArgumentSchema,
    INamedTypeSymbol NamedTypeSymbol,
    IReadOnlyList<AttributeSyntax> AttributeList,
    string Separator)
    : PropertySchemaBase(GenericArgumentSchema.PropertyName, NamedTypeSymbol, AttributeList)
{
    protected override int OnCheckCompatibility(ICompatibilityContext context, ILogger logger)
    {
        var arguments = context.CurrentArgument.Split(Separator);

        var hashSet = arguments.ToHashSet();
        if (hashSet.Count != arguments.Length)
        {
            var ex = new InvalidOperationException(
                $"Parameter {PropertyName} has duplicate values in the argument: {context}");
            LogError(logger, GetType(), context.ToString(), ex, ex.InnerException);
            throw ex;
        }

        foreach (var argument in arguments)
        {
            if (string.IsNullOrWhiteSpace(argument))
            {
                var ex = new InvalidOperationException(
                    $"Parameter {PropertyName} has empty value in the argument: {context}");
                LogError(logger, GetType(), context.ToString(), ex, ex.InnerException);
                throw ex;
            }

            var nestedContext = new CompatibilityContext(context.EnumMemberCatalog, [argument]);
            GenericArgumentSchema.CheckCompatibility(nestedContext, logger);
        }

        return 1;
    }
}
