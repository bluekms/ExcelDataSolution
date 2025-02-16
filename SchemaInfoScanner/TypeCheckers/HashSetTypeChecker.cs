using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.Exceptions;
using SchemaInfoScanner.Extensions;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.Schemata;
using StaticDataAttribute;

namespace SchemaInfoScanner.TypeCheckers;

internal static class HashSetTypeChecker
{
    private static readonly HashSet<string> SupportedTypeNames = new()
    {
        "HashSet<>",
        "ImmutableHashSet<>",
        "ImmutableSortedSet<>",
    };

    public static void Check(
        RawParameterSchema rawParameter,
        RecordSchemaContainer recordSchemaContainer,
        HashSet<RecordName> visited,
        ILogger logger)
    {
        if (!IsSupportedHashSetType(rawParameter.NamedTypeSymbol))
        {
            throw new InvalidOperationException($"Expected {rawParameter.ParameterName.FullName} to be supported hashset type, but actually not supported.");
        }

        CheckUnavailableAttribute(rawParameter);

        var typeArgument = (INamedTypeSymbol)rawParameter.NamedTypeSymbol.TypeArguments.Single();
        if (PrimitiveTypeChecker.IsSupportedPrimitiveType(typeArgument))
        {
            return;
        }

        if (typeArgument.NullableAnnotation is NullableAnnotation.Annotated)
        {
            throw new TypeNotSupportedException($"{rawParameter.ParameterName.FullName} is not supported hashset type. Nullable record item for hashset is not supported.");
        }

        if (rawParameter.HasAttribute<SingleColumnContainerAttribute>())
        {
            throw new TypeNotSupportedException($"{rawParameter.ParameterName.FullName} is not supported hashset type. {nameof(SingleColumnContainerAttribute)} can only be used in primitive type hashset.");
        }

        var innerRecordSchema = rawParameter.FindInnerRecordSchema(recordSchemaContainer);
        RecordTypeChecker.Check(innerRecordSchema, recordSchemaContainer, visited, logger);
    }

    private static void CheckUnavailableAttribute(RawParameterSchema rawParameter)
    {
        if (rawParameter.HasAttribute<ForeignKeyAttribute>())
        {
            throw new InvalidUsageException($"{nameof(ForeignKeyAttribute)} is not available for hashset type {rawParameter.ParameterName.FullName}.");
        }

        if (rawParameter.HasAttribute<KeyAttribute>())
        {
            throw new InvalidUsageException($"{nameof(KeyAttribute)} is not available for hashset type {rawParameter.ParameterName.FullName}.");
        }

        if (rawParameter.HasAttribute<NullStringAttribute>())
        {
            throw new InvalidUsageException($"{nameof(NullStringAttribute)} is not available for hashset type {rawParameter.ParameterName.FullName}.");
        }
    }

    public static bool IsSupportedHashSetType(INamedTypeSymbol symbol)
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
