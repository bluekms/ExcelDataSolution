using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.Exceptions;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.Schemata;
using StaticDataAttribute;

namespace SchemaInfoScanner.TypeCheckers;

public static class HashSetTypeChecker
{
    private static readonly HashSet<string> SupportedTypeNames = new()
    {
        "HashSet<>",
        "ImmutableHashSet<>",
        "ImmutableSortedSet<>",
    };

    public static bool IsSupportedHashSetType(INamedTypeSymbol symbol)
    {
        if (symbol.TypeArguments is not [INamedTypeSymbol] ||
            symbol.OriginalDefinition.SpecialType is SpecialType.System_Nullable_T)
        {
            return false;
        }

        var genericTypeDefinitionName = symbol
            .ConstructUnboundGenericType()
            .ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        return SupportedTypeNames.Contains(genericTypeDefinitionName);
    }

    public static bool IsSupportedHashSetType(RecordParameterSchema recordParameter)
    {
        return IsSupportedHashSetType(recordParameter.NamedTypeSymbol);
    }

    public static void Check(
        RecordParameterSchema recordParameter,
        RecordSchemaContainer recordSchemaContainer,
        HashSet<RecordName> visited,
        ILogger logger)
    {
        if (!IsSupportedHashSetType(recordParameter))
        {
            throw new TypeNotSupportedException($"{recordParameter.ParameterName.FullName} is not supported hashset type.");
        }

        CheckUnavailableAttribute(recordParameter);

        if (recordParameter.NamedTypeSymbol.TypeArguments.First() is not INamedTypeSymbol typeArgument)
        {
            throw new TypeNotSupportedException($"{recordParameter.ParameterName.FullName} is not supported hashset type. Type argument is null.");
        }

        if (!PrimitiveTypeChecker.IsSupportedPrimitiveType(typeArgument))
        {
            if (recordParameter.HasAttribute<SingleColumnContainerAttribute>())
            {
                throw new TypeNotSupportedException($"{recordParameter.ParameterName.FullName} is not supported hashset type. {nameof(SingleColumnContainerAttribute)} can only be used in primitive type hashset.");
            }

            if (typeArgument.NullableAnnotation is NullableAnnotation.Annotated)
            {
                throw new TypeNotSupportedException($"{recordParameter.ParameterName.FullName} is not supported hashset type. Nullable type argument is not supported.");
            }

            if (ContainerTypeChecker.IsContainerType(typeArgument))
            {
                throw new TypeNotSupportedException($"{recordParameter.ParameterName.FullName} is not supported hashset type. Nested container type argument is not supported.");
            }

            var recordName = new RecordName(typeArgument);
            if (!recordSchemaContainer.RecordSchemaDictionary.TryGetValue(recordName, out var typeArgumentSchema))
            {
                var innerException = new KeyNotFoundException($"{recordName.FullName} is not found in the RecordSchemaDictionary");
                throw new TypeNotSupportedException($"{recordParameter.ParameterName.FullName} is not supported type.", innerException);
            }

            RecordTypeChecker.Check(typeArgumentSchema, recordSchemaContainer, visited, logger);
        }
    }

    private static void CheckUnavailableAttribute(RecordParameterSchema recordParameter)
    {
        if (recordParameter.HasAttribute<ColumnNameAttribute>())
        {
            throw new InvalidUsageException($"{nameof(ColumnNameAttribute)} is not available for hashset type {recordParameter.ParameterName.FullName}.");
        }

        if (recordParameter.HasAttribute<ForeignKeyAttribute>())
        {
            throw new InvalidUsageException($"{nameof(ForeignKeyAttribute)} is not available for hashset type {recordParameter.ParameterName.FullName}.");
        }

        if (recordParameter.HasAttribute<KeyAttribute>())
        {
            throw new InvalidUsageException($"{nameof(KeyAttribute)} is not available for hashset type {recordParameter.ParameterName.FullName}.");
        }

        if (recordParameter.HasAttribute<NullStringAttribute>())
        {
            throw new InvalidUsageException($"{nameof(NullStringAttribute)} is not available for hashset type {recordParameter.ParameterName.FullName}.");
        }

        if (recordParameter.HasAttribute<SingleColumnContainerAttribute>())
        {
            if (recordParameter.HasAttribute<ColumnPrefixAttribute>())
            {
                throw new InvalidUsageException($"{nameof(SingleColumnContainerAttribute)} overwrites {nameof(ColumnPrefixAttribute)}. {recordParameter.ParameterName.FullName}.");
            }
        }
    }
}
