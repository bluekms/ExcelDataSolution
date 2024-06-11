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
    public static bool IsSupportedDictionaryType(INamedTypeSymbol symbol)
    {
        // TODO: ImmutableDictionary, FrozenDictionary도 지원할 것
        return symbol.Name.StartsWith("Dictionary", StringComparison.Ordinal) &&
               symbol.TypeArguments is [INamedTypeSymbol, INamedTypeSymbol] &&
               symbol.OriginalDefinition.SpecialType is not SpecialType.System_Nullable_T;
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
        if (!IsSupportedDictionaryType(recordParameter))
        {
            throw new TypeNotSupportedException($"{recordParameter.ParameterName.FullName} is not supported dictionary type.");
        }

        CheckUnavailableAttribute(recordParameter);

        if (recordParameter.NamedTypeSymbol.TypeArguments is not [INamedTypeSymbol keySymbol, INamedTypeSymbol valueSymbol])
        {
            throw new TypeNotSupportedException($"{recordParameter.ParameterName.FullName} is not INamedTypeSymbol for dictionary key.");
        }

        IsDictionaryKeySupport(keySymbol, recordSchemaContainer, visited, logger);
        IsDictionaryValueSupport(keySymbol, valueSymbol, recordSchemaContainer, visited, logger);
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

    private static void IsDictionaryKeySupport(
        INamedTypeSymbol symbol,
        RecordSchemaContainer recordSchemaContainer,
        HashSet<RecordName> visited,
        ILogger logger)
    {
        if (symbol.OriginalDefinition.SpecialType is SpecialType.System_Nullable_T)
        {
            throw new TypeNotSupportedException($"Not support nullable key for dictionary.");
        }

        if (ContainerTypeChecker.IsContainerType(symbol))
        {
            throw new TypeNotSupportedException($"Not support container type for dictionary key.");
        }

        var specialTypeCheck = PrimitiveTypeChecker.CheckSpecialType(symbol.SpecialType);
        var typeKindCheck = PrimitiveTypeChecker.CheckEnumType(symbol.TypeKind);

        if (!(specialTypeCheck || typeKindCheck))
        {
            if (symbol.NullableAnnotation is NullableAnnotation.Annotated)
            {
                throw new TypeNotSupportedException($"Nullable key is not supported for dictionary.");
            }

            var recordName = new RecordName(symbol);
            if (!recordSchemaContainer.RecordSchemaDictionary.TryGetValue(recordName, out var typeArgumentSchema))
            {
                var innerException = new KeyNotFoundException($"{recordName.FullName} is not found in the RecordSchemaDictionary");
                throw new TypeNotSupportedException($"{recordName.FullName} is not supported type.", innerException);
            }

            RecordTypeChecker.Check(typeArgumentSchema, recordSchemaContainer, visited, logger);
        }
    }

    private static void IsDictionaryValueSupport(
        INamedTypeSymbol keySymbol,
        INamedTypeSymbol valueSymbol,
        RecordSchemaContainer recordSchemaContainer,
        HashSet<RecordName> visited,
        ILogger logger)
    {
        if (PrimitiveTypeChecker.IsSupportedPrimitiveType(valueSymbol))
        {
            throw new TypeNotSupportedException($"Primitive value is not supported for dictionary.");
        }

        if (valueSymbol.NullableAnnotation is NullableAnnotation.Annotated)
        {
            throw new TypeNotSupportedException($"Nullable value is not supported for dictionary.");
        }

        if (valueSymbol.TypeArguments.Any())
        {
            throw new TypeNotSupportedException($"Not support container type for dictionary value.");
        }

        var valueRecordName = new RecordName(valueSymbol);
        if (!recordSchemaContainer.RecordSchemaDictionary.TryGetValue(valueRecordName, out var valueRecordSchema))
        {
            var innerException = new KeyNotFoundException($"{valueRecordName.FullName} is not found in the RecordSchemaDictionary");
            throw new TypeNotSupportedException($"{valueRecordName.FullName} is not supported type.", innerException);
        }

        RecordTypeChecker.Check(valueRecordSchema, recordSchemaContainer, visited, logger);

        var valueRecordKeyParameterSchema = valueRecordSchema.RecordParameterSchemaList
            .Single(x => x.HasAttribute<KeyAttribute>());

        if (PrimitiveTypeChecker.IsSupportedPrimitiveType(valueRecordKeyParameterSchema))
        {
            CheckSamePrimitiveType(keySymbol, valueRecordKeyParameterSchema.NamedTypeSymbol);
            return;
        }

        if (valueSymbol.NullableAnnotation is NullableAnnotation.Annotated)
        {
            throw new TypeNotSupportedException($"Nullable value is not supported for dictionary.");
        }

        SupportedTypeChecker.Check(valueRecordKeyParameterSchema, recordSchemaContainer, visited, logger);
        CheckSameRecordType(keySymbol, valueRecordKeyParameterSchema);
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

    private static void CheckSameRecordType(INamedTypeSymbol keySymbol, RecordParameterSchema valueRecordKeyParameterSchema)
    {
        var dictionaryKeyRecordName = new RecordName(keySymbol);
        var valueKeyRecordName = new RecordName(valueRecordKeyParameterSchema.NamedTypeSymbol);

        if (!dictionaryKeyRecordName.Equals(valueKeyRecordName))
        {
            throw new TypeNotSupportedException($"Key and value type of dictionary must be same type.");
        }
    }
}
