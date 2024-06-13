using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.Exceptions;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.Schemata;
using StaticDataAttribute;

namespace SchemaInfoScanner.TypeCheckers;

public static class DictionaryTypeChecker
{
    private static readonly HashSet<string> SupportedTypeNames = new()
    {
        "Dictionary<, >",
        "ImmutableDictionary<, >",
        "ImmutableSortedDictionary<, >",
        "FrozenDictionary<, >",
    };

    public static bool IsSupportedDictionaryType(INamedTypeSymbol symbol)
    {
        if (symbol.TypeArguments is not [INamedTypeSymbol, INamedTypeSymbol] ||
            symbol.OriginalDefinition.SpecialType is SpecialType.System_Nullable_T)
        {
            return false;
        }

        var genericTypeDefinitionName = symbol
            .ConstructUnboundGenericType()
            .ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        return SupportedTypeNames.Contains(genericTypeDefinitionName);
    }

    public static bool IsSupportedDictionaryType(RecordParameterSchema recordParameter)
    {
        return IsSupportedDictionaryType(recordParameter.NamedTypeSymbol);
    }

    public static void Check(
        RecordParameterSchema recordParameter,
        RecordSchemaContainer recordSchemaContainer,
        HashSet<RecordName> visited,
        ILogger logger)
    {
        var parameterType = RecordParameterTypeInferencer.Infer(recordParameter.NamedTypeSymbol);
        if (parameterType is not IDictionaryType dictionaryParameter)
        {
            throw new InvalidOperationException($"Expected infer result to be {nameof(IDictionaryType)}, but actually {parameterType.GetType().FullName}.");
        }

        CheckUnavailableAttribute(recordParameter);

        var keySymbol = (INamedTypeSymbol)recordParameter.NamedTypeSymbol.TypeArguments[0];
        var valueSymbol = (INamedTypeSymbol)recordParameter.NamedTypeSymbol.TypeArguments[1];

        var valueRecordSchema = RecordTypeChecker.CheckAndGetSchema(valueSymbol, recordSchemaContainer, visited, logger);

        var valueRecordKeyParameterSchema = valueRecordSchema.RecordParameterSchemaList
            .Single(x => x.HasAttribute<KeyAttribute>());

        if (dictionaryParameter.KeyItem() is RecordType)
        {
            var keyRecordSchema = RecordTypeChecker.CheckAndGetSchema(keySymbol, recordSchemaContainer, visited, logger);

            var valueRecordKeyParameterRecordName = new RecordName(valueRecordKeyParameterSchema.NamedTypeSymbol);
            if (!keyRecordSchema.RecordName.Equals(valueRecordKeyParameterRecordName))
            {
                throw new TypeNotSupportedException($"Key and value type of dictionary must be same type.");
            }

            return;
        }

        CheckSamePrimitiveType(keySymbol, valueRecordKeyParameterSchema.NamedTypeSymbol);
    }

    private static void CheckUnavailableAttribute(RecordParameterSchema recordParameter)
    {
        if (recordParameter.HasAttribute<ColumnNameAttribute>())
        {
            throw new InvalidUsageException($"{nameof(ColumnNameAttribute)} is not available for dictionary type {recordParameter.ParameterName.FullName}.");
        }

        if (recordParameter.HasAttribute<ForeignKeyAttribute>())
        {
            throw new InvalidUsageException($"{nameof(ForeignKeyAttribute)} is not available for dictionary type {recordParameter.ParameterName.FullName}.");
        }

        if (recordParameter.HasAttribute<KeyAttribute>())
        {
            throw new InvalidUsageException($"{nameof(KeyAttribute)} is not available for dictionary type {recordParameter.ParameterName.FullName}.");
        }

        if (recordParameter.HasAttribute<NullStringAttribute>())
        {
            throw new InvalidUsageException($"{nameof(NullStringAttribute)} is not available for dictionary type {recordParameter.ParameterName.FullName}.");
        }

        if (recordParameter.HasAttribute<SingleColumnContainerAttribute>())
        {
            throw new InvalidUsageException($"{nameof(NullStringAttribute)} is not available for dictionary type {recordParameter.ParameterName.FullName}.");
        }
    }

    private static void CheckSamePrimitiveType(INamedTypeSymbol keySymbol, INamedTypeSymbol valueSymbol)
    {
        if (keySymbol.SpecialType is not SpecialType.None &&
            keySymbol.SpecialType == valueSymbol.SpecialType)
        {
            return;
        }

        if (keySymbol.TypeKind is not TypeKind.Enum || valueSymbol.TypeKind is not TypeKind.Enum)
        {
            throw new TypeNotSupportedException($"Key and value type of dictionary must be same type.");
        }

        if (keySymbol.Name != valueSymbol.Name)
        {
            throw new TypeNotSupportedException($"Key and value type of dictionary must be same type.");
        }
    }
}
