using Microsoft.CodeAnalysis;
using SchemaInfoScanner.Exceptions;
using SchemaInfoScanner.Schemata;
using StaticDataAttribute;

namespace SchemaInfoScanner.TypeCheckers;

public static class HashSetTypeChecker
{
    public static bool IsSupportedHashSetType(RecordParameterSchema recordParameter)
    {
        return recordParameter.NamedTypeSymbol.Name.StartsWith("HashSet", StringComparison.Ordinal) &&
               recordParameter.NamedTypeSymbol.TypeArguments is [INamedTypeSymbol];
    }

    public static void Check(RecordParameterSchema recordParameter)
    {
        if (!IsSupportedHashSetType(recordParameter))
        {
            throw new TypeNotSupportedException($"{recordParameter.ParameterName.FullName} is not supported hash set type.");
        }

        CheckAttributes(recordParameter);

        try
        {
            PrimitiveTypeChecker.Check(recordParameter);
        }
        catch (Exception e)
        {
            throw new TypeNotSupportedException($"Not support hash set with not primitive type. Use List Or Dictionary.", e);
        }
    }

    private static void CheckAttributes(RecordParameterSchema recordParameter)
    {
        if (recordParameter.HasAttribute<ColumnNameAttribute>())
        {
            throw new InvalidUsageException("ColumnNameAttribute is not supported for hash set type. Use ColumnPrefixAttribute or ColumnSuffixAttribute instead.");
        }

        if (!recordParameter.HasAttribute<ColumnPrefixAttribute>() &&
            !recordParameter.HasAttribute<ColumnSuffixAttribute>())
        {
            throw new InvalidUsageException("ColumnPrefixAttribute or ColumnSuffixAttribute is required for hash set type.");
        }
    }
}
