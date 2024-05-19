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
    public static bool IsSupportedListType(RecordParameterSchema recordParameter)
    {
        return recordParameter.NamedTypeSymbol.Name.StartsWith("List", StringComparison.Ordinal) &&
               recordParameter.NamedTypeSymbol.TypeArguments is [INamedTypeSymbol];
    }

    public static void Check(
        RecordParameterSchema recordParameter,
        RecordSchemaContainer recordSchemaContainer,
        SemanticModelContainer semanticModelContainer,
        HashSet<RecordName> visited,
        ILogger logger)
    {
        if (!IsSupportedListType(recordParameter))
        {
            throw new TypeNotSupportedException($"{recordParameter.ParameterName.FullName} is not supported list type.");
        }

        CheckAttributes(recordParameter);

        if (recordParameter.NamedTypeSymbol.TypeArguments.First() is not INamedTypeSymbol typeArgument)
        {
            throw new TypeNotSupportedException($"{recordParameter.ParameterName.FullName} is not supported list type. Type argument is null.");
        }

        if (!PrimitiveTypeChecker.IsSupportedPrimitiveType(typeArgument))
        {
            var recordName = new RecordName(typeArgument);
            var typeArgumentSchema = recordSchemaContainer.RecordSchemaDictionary[recordName];

            RecordTypeChecker.Check(typeArgumentSchema, recordSchemaContainer, semanticModelContainer, visited, logger);
        }
    }

    private static void CheckAttributes(RecordParameterSchema recordParameter)
    {
        if (recordParameter.HasAttribute<ColumnNameAttribute>())
        {
            throw new InvalidUsageException("ColumnNameAttribute is not supported for list type. Use ColumnPrefixAttribute or ColumnSuffixAttribute instead.");
        }

        if (!recordParameter.HasAttribute<ColumnPrefixAttribute>() &&
            !recordParameter.HasAttribute<ColumnSuffixAttribute>())
        {
            throw new InvalidUsageException("ColumnPrefixAttribute or ColumnSuffixAttribute is required for list type.");
        }
    }
}
