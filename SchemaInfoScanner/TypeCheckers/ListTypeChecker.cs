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
    private static readonly HashSet<string> SupportedTypeNames = new()
    {
        "List<>",
        "ImmutableList<>",
        "ImmutableArray<>",
        "SortedList<>",
    };

    public static bool IsSupportedListType(INamedTypeSymbol symbol)
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
        var parameterType = RecordParameterTypeInferencer.Infer(recordParameter.NamedTypeSymbol);
        if (parameterType is not IListType listParameter)
        {
            throw new InvalidOperationException($"Expected infer result to be {nameof(IListType)}, but actually {parameterType.GetType().FullName}.");
        }

        CheckUnavailableAttribute(recordParameter);

        if (listParameter.Item() is PrimitiveType or NullablePrimitiveType)
        {
            return;
        }

        if (recordParameter.HasAttribute<SingleColumnContainerAttribute>())
        {
            throw new TypeNotSupportedException($"{recordParameter.ParameterName.FullName} is not supported list type. {nameof(SingleColumnContainerAttribute)} can only be used in primitive type list.");
        }

        var typeArgument = (INamedTypeSymbol)recordParameter.NamedTypeSymbol.TypeArguments.Single();
        var recordName = new RecordName(typeArgument);
        if (!recordSchemaContainer.RecordSchemaDictionary.TryGetValue(recordName, out var typeArgumentSchema))
        {
            var innerException = new KeyNotFoundException($"{recordName.FullName} is not found in the RecordSchemaDictionary");
            throw new TypeNotSupportedException($"{recordParameter.ParameterName.FullName} is not supported type.", innerException);
        }

        RecordTypeChecker.Check(typeArgumentSchema, recordSchemaContainer, visited, logger);
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
