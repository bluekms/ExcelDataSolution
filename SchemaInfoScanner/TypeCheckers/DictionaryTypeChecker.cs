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
        return symbol.Name.StartsWith("Dictionary", StringComparison.Ordinal) &&
               symbol.TypeArguments is [INamedTypeSymbol, INamedTypeSymbol];
    }

    public static bool IsSupportedDictionaryType(RecordParameterSchema recordParameter)
    {
        return IsSupportedDictionaryType(recordParameter.NamedTypeSymbol);
    }

    public static void Check(
        RecordParameterSchema recordParameter,
        RecordSchemaContainer recordSchemaContainer,
        SemanticModelContainer semanticModelContainer,
        HashSet<RecordName> visited,
        ILogger logger)
    {
        if (!IsSupportedDictionaryType(recordParameter))
        {
            throw new TypeNotSupportedException($"{recordParameter.ParameterName.FullName} is not supported dictionary type.");
        }

        CheckAttribute(recordParameter);

        if (recordParameter.NamedTypeSymbol.TypeArguments is not [INamedTypeSymbol keySymbol, INamedTypeSymbol valueSymbol])
        {
            throw new TypeNotSupportedException($"{recordParameter.ParameterName.FullName} is not INamedTypeSymbol for dictionary key.");
        }

        IsDictionaryKeySupport(keySymbol, recordSchemaContainer, semanticModelContainer, visited, logger);
        IsDictionaryValueSupport(keySymbol, valueSymbol, recordSchemaContainer, semanticModelContainer, visited, logger);
    }

    private static void CheckAttribute(RecordParameterSchema recordParameter)
    {
        if (recordParameter.HasAttribute<ColumnNameAttribute>())
        {
            throw new InvalidUsageException("ColumnNameAttribute is not supported for dictionary type. Use ColumnPrefixAttribute or ColumnSuffixAttribute instead.");
        }

        if (!recordParameter.HasAttribute<ColumnPrefixAttribute>() &&
            !recordParameter.HasAttribute<ColumnSuffixAttribute>())
        {
            throw new InvalidUsageException("ColumnPrefixAttribute or ColumnSuffixAttribute is required for dictionary type.");
        }
    }

    private static void IsDictionaryKeySupport(
        INamedTypeSymbol symbol,
        RecordSchemaContainer recordSchemaContainer,
        SemanticModelContainer semanticModelContainer,
        HashSet<RecordName> visited,
        ILogger logger)
    {
        if (symbol.OriginalDefinition.SpecialType is SpecialType.System_Nullable_T)
        {
            throw new TypeNotSupportedException($"Not support nullable key for dictionary.");
        }

        if (ContainerTypeChecker.IsSupportedContainerType(symbol))
        {
            throw new TypeNotSupportedException($"Not support container type for dictionary key.");
        }

        var specialTypeCheck = PrimitiveTypeChecker.CheckSpecialType(symbol.SpecialType);
        var typeKindCheck = PrimitiveTypeChecker.CheckEnumType(symbol.TypeKind);

        if (!(specialTypeCheck || typeKindCheck))
        {
            var recordName = new RecordName(symbol);
            var typeArgumentSchema = recordSchemaContainer.RecordSchemaDictionary[recordName];

            RecordTypeChecker.Check(typeArgumentSchema, recordSchemaContainer, semanticModelContainer, visited, logger);
        }
    }

    private static void IsDictionaryValueSupport(
        INamedTypeSymbol keySymbol,
        INamedTypeSymbol valueSymbol,
        RecordSchemaContainer recordSchemaContainer,
        SemanticModelContainer semanticModelContainer,
        HashSet<RecordName> visited,
        ILogger logger)
    {
        if (PrimitiveTypeChecker.IsSupportedPrimitiveType(valueSymbol))
        {
            return;
        }

        var valueRecordName = new RecordName(valueSymbol);
        var valueRecordSchema = recordSchemaContainer.RecordSchemaDictionary[valueRecordName];

        RecordTypeChecker.Check(valueRecordSchema, recordSchemaContainer, semanticModelContainer, visited, logger);

        var valueRecordKeyParameterSchema = valueRecordSchema.RecordParameterSchemaList
            .Single(x => x.HasAttribute<KeyAttribute>());

        if (PrimitiveTypeChecker.IsSupportedPrimitiveType(valueRecordKeyParameterSchema))
        {
            CheckSamePrimitiveType(keySymbol, valueRecordKeyParameterSchema.NamedTypeSymbol);
            return;
        }

        SupportedTypeChecker.Check(valueRecordKeyParameterSchema, recordSchemaContainer, semanticModelContainer, visited, logger);
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
