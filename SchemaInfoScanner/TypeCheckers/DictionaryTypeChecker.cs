using System.ComponentModel.DataAnnotations;
using Microsoft.CodeAnalysis;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.Exceptions;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.Schemata;
using StaticDataAttribute;
using KeyAttribute = System.ComponentModel.DataAnnotations.KeyAttribute;

namespace SchemaInfoScanner.TypeCheckers;

public static class DictionaryTypeChecker
{
    public static bool IsSupportedDictionaryType(RecordParameterSchema recordParameter)
    {
        return recordParameter.NamedTypeSymbol.Name.StartsWith("Dictionary", StringComparison.Ordinal) &&
               recordParameter.NamedTypeSymbol.TypeArguments is [INamedTypeSymbol, INamedTypeSymbol];
    }

    public static void Check(RecordParameterSchema recordParameter, RecordSchemaContainer recordSchemaContainer, SemanticModelContainer semanticModelContainer)
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

        IsDictionaryKeyPrimitive(keySymbol);
        IsDictionaryValueSupport(keySymbol, valueSymbol, recordSchemaContainer, semanticModelContainer);
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

    private static void IsDictionaryKeyPrimitive(INamedTypeSymbol symbol)
    {
        if (symbol.OriginalDefinition.SpecialType is SpecialType.System_Nullable_T)
        {
            throw new TypeNotSupportedException($"Not support nullable key for dictionary.");
        }

        var specialTypeCheck = PrimitiveTypeChecker.CheckSpecialType(symbol.SpecialType);
        var typeKindCheck = PrimitiveTypeChecker.CheckEnumType(symbol.TypeKind);

        if (!(specialTypeCheck || typeKindCheck))
        {
            throw new TypeNotSupportedException($"Not support dictionary with not primitive type key.");
        }
    }

    private static void IsDictionaryValueSupport(INamedTypeSymbol keySymbol, INamedTypeSymbol valueSymbol, RecordSchemaContainer recordSchemaContainer, SemanticModelContainer semanticModelContainer)
    {
        if (PrimitiveTypeChecker.IsSupportedPrimitiveType(valueSymbol))
        {
            return;
        }

        var valueRecordName = new RecordName(valueSymbol);
        var valueRecordSchema = recordSchemaContainer.RecordSchemaDictionary[valueRecordName];

        RecordTypeChecker.Check(valueRecordSchema, recordSchemaContainer, semanticModelContainer);

        var valueRecordKeyParameterSchema = valueRecordSchema.RecordParameterSchemaList
            .Single(x => x.HasAttribute<KeyAttribute>());

        PrimitiveTypeChecker.Check(valueRecordKeyParameterSchema);

        CheckSameType(keySymbol, valueRecordKeyParameterSchema.NamedTypeSymbol);
    }

    private static void CheckSameType(INamedTypeSymbol keySymbol, INamedTypeSymbol valueSymbol)
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
