using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner.Schemata.AttributeCheckers;
using SchemaInfoScanner.Schemata.CompatibilityContexts;

namespace SchemaInfoScanner.Schemata.TypedPropertySchemata.CollectionTypes.NullableTypes;

public sealed record NullablePrimitiveHashSetPropertySchema(
    PrimitiveTypeGenericArgumentSchema GenericArgumentSchema,
    INamedTypeSymbol NamedTypeSymbol,
    IReadOnlyList<AttributeSyntax> AttributeList)
    : PropertySchemaBase(GenericArgumentSchema.PropertyName, NamedTypeSymbol, AttributeList)
{
    protected override int OnCheckCompatibility(ICompatibilityContext context, ILogger logger)
    {
        if (!context.IsCollection)
        {
            throw new InvalidOperationException($"Invalid context: {context}");
        }

        var totalConsumed = 0;
        var arguments = new List<string>();
        for (var i = 0; i < context.CollectionLength; i++)
        {
            var nestedContext = context.WithStartIndex(context.StartIndex + totalConsumed);
            arguments.Add(nestedContext.CurrentArgument);

            var result = NullStringAttributeChecker.Check(this, nestedContext.CurrentArgument);
            if (result.IsNull)
            {
                totalConsumed += 1;
            }
            else
            {
                totalConsumed += GenericArgumentSchema.CheckCompatibility(nestedContext, logger);
            }
        }

        var hasDuplicates = arguments
            .GroupBy(x => x)
            .Any(x => x.Count() > 1);

        if (hasDuplicates)
        {
            var ex = new InvalidOperationException(
                $"Parameter {PropertyName} has duplicate values in the argument: {context}");
            LogError(logger, GetType(), context.ToString(), ex, ex.InnerException);
            throw ex;
        }

        return totalConsumed;
    }
}
