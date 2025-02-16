using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.Exceptions;
using SchemaInfoScanner.Extensions;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.Schemata;
using StaticDataAttribute;

namespace SchemaInfoScanner.TypeCheckers;

internal static class ListTypeChecker
{
    private static readonly HashSet<string> SupportedTypeNames = new()
    {
        "List<>",
        "ImmutableList<>",
        "ImmutableArray<>",
        "SortedList<>",
    };

    public static void Check(
        RawParameterSchema rawParameter,
        RecordSchemaContainer recordSchemaContainer,
        HashSet<RecordName> visited,
        ILogger logger)
    {
        if (!IsSupportedListType(rawParameter.NamedTypeSymbol))
        {
            throw new InvalidOperationException($"Expected {rawParameter.ParameterName.FullName} to be supported list type, but actually not supported.");
        }

        CheckUnavailableAttribute(rawParameter);

        var typeArgument = (INamedTypeSymbol)rawParameter.NamedTypeSymbol.TypeArguments.Single();
        if (PrimitiveTypeChecker.IsSupportedPrimitiveType(typeArgument))
        {
            return;
        }

        if (typeArgument.NullableAnnotation is NullableAnnotation.Annotated)
        {
            throw new TypeNotSupportedException($"{rawParameter.ParameterName.FullName} is not supported list type. Nullable record item for list is not supported.");
        }

        if (rawParameter.HasAttribute<SingleColumnContainerAttribute>())
        {
            throw new TypeNotSupportedException($"{rawParameter.ParameterName.FullName} is not supported list type. {nameof(SingleColumnContainerAttribute)} can only be used in primitive type list.");
        }

        var innerRecordSchema = rawParameter.FindInnerRecordSchema(recordSchemaContainer);
        RecordTypeChecker.Check(innerRecordSchema, recordSchemaContainer, visited, logger);
    }

    private static void CheckUnavailableAttribute(RawParameterSchema rawParameter)
    {
        if (rawParameter.HasAttribute<ForeignKeyAttribute>())
        {
            throw new InvalidUsageException($"{nameof(ForeignKeyAttribute)} is not available for list type {rawParameter.ParameterName.FullName}.");
        }

        if (rawParameter.HasAttribute<KeyAttribute>())
        {
            throw new InvalidUsageException($"{nameof(KeyAttribute)} is not available for list type {rawParameter.ParameterName.FullName}.");
        }

        if (rawParameter.HasAttribute<NullStringAttribute>())
        {
            throw new InvalidUsageException($"{nameof(NullStringAttribute)} is not available for list type {rawParameter.ParameterName.FullName}.");
        }
    }

    public static bool IsSupportedListType(INamedTypeSymbol symbol)
    {
        if (symbol.OriginalDefinition.SpecialType is SpecialType.System_Nullable_T ||
            symbol.TypeArguments is not [INamedTypeSymbol])
        {
            return false;
        }

        var genericTypeDefinitionName = symbol
            .ConstructUnboundGenericType()
            .ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        return SupportedTypeNames.Contains(genericTypeDefinitionName);
    }
}
