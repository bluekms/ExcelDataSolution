using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.Exceptions;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.Schemata;
using StaticDataAttribute;

namespace SchemaInfoScanner.TypeCheckers;

public static class ListTypeChecker
{
    public static bool IsSupportedListType(INamedTypeSymbol symbol)
    {
        // TODO: ReadonlyList, ImmutableList도 지원할 것
        return symbol.Name.StartsWith("List", StringComparison.Ordinal) &&
               symbol.TypeArguments is [INamedTypeSymbol] &&
               symbol.OriginalDefinition.SpecialType is not SpecialType.System_Nullable_T;
    }

    public static bool IsSupportedListType(RecordParameterSchema recordParameter)
    {
        return IsSupportedListType(recordParameter.NamedTypeSymbol);
    }

    public static void Check(
        RecordParameterSchema recordParameter,
        RecordSchemaContainer recordSchemaContainer,
        HashSet<RecordName> visited,
        ILogger logger)
    {
        if (!IsSupportedListType(recordParameter))
        {
            throw new TypeNotSupportedException($"{recordParameter.ParameterName.FullName} is not supported list type.");
        }

        CheckUnavailableAttribute(recordParameter);

        if (recordParameter.NamedTypeSymbol.TypeArguments.First() is not INamedTypeSymbol typeArgument)
        {
            throw new TypeNotSupportedException($"{recordParameter.ParameterName.FullName} is not supported list type. Type argument is null.");
        }

        if (!PrimitiveTypeChecker.IsSupportedPrimitiveType(typeArgument))
        {
            if (recordParameter.HasAttribute<SingleColumnContainerAttribute>())
            {
                throw new TypeNotSupportedException($"{recordParameter.ParameterName.FullName} is not supported list type. {nameof(SingleColumnContainerAttribute)} can only be used in primitive type list.");
            }

            if (typeArgument.NullableAnnotation is NullableAnnotation.Annotated)
            {
                throw new TypeNotSupportedException($"{recordParameter.ParameterName.FullName} is not supported list type. Nullable type argument is not supported.");
            }

            if (ContainerTypeChecker.IsContainerType(typeArgument))
            {
                throw new TypeNotSupportedException($"{recordParameter.ParameterName.FullName} is not supported list type. Nested container type argument is not supported.");
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
            throw new InvalidUsageException($"{nameof(ColumnNameAttribute)} is not available for list type {recordParameter.ParameterName.FullName}.");
        }

        if (recordParameter.HasAttribute<ForeignKeyAttribute>())
        {
            throw new InvalidUsageException($"{nameof(ForeignKeyAttribute)} is not available for list type {recordParameter.ParameterName.FullName}.");
        }

        if (recordParameter.HasAttribute<KeyAttribute>())
        {
            throw new InvalidUsageException($"{nameof(KeyAttribute)} is not available for list type {recordParameter.ParameterName.FullName}.");
        }

        if (recordParameter.HasAttribute<NullStringAttribute>())
        {
            throw new InvalidUsageException($"{nameof(NullStringAttribute)} is not available for list type {recordParameter.ParameterName.FullName}.");
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
