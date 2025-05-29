using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner.Catalogs;
using SchemaInfoScanner.Exceptions;
using SchemaInfoScanner.Extensions;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.Schemata;
using StaticDataAttribute;

namespace SchemaInfoScanner.TypeCheckers;

internal static class DictionaryTypeChecker
{
    private static readonly HashSet<string> SupportedTypeNames =
    [
        "Dictionary<, >",
        "ReadOnlyDictionary<, >",
        "ImmutableDictionary<, >",
        "ImmutableSortedDictionary<, >",
        "FrozenDictionary<, >"
    ];

    public static void Check(
        PropertySchemaBase property,
        RecordSchemaCatalog recordSchemaCatalog,
        HashSet<RecordName> visited,
        ILogger logger)
    {
        if (!IsSupportedDictionaryType(property.NamedTypeSymbol))
        {
            throw new InvalidOperationException($"Expected {property.PropertyName.FullName} to be supported dictionary type, but actually not supported.");
        }

        CheckUnavailableAttribute(property);

        var keySymbol = (INamedTypeSymbol)property.NamedTypeSymbol.TypeArguments[0];
        if (keySymbol.NullableAnnotation is NullableAnnotation.Annotated)
        {
            throw new NotSupportedException($"Key type of dictionary must be non-nullable.");
        }

        if (PrimitiveTypeChecker.IsDateTimeType(keySymbol))
        {
            if (!property.HasAttribute<DateTimeFormatAttribute>())
            {
                throw new AttributeNotFoundException<DateTimeFormatAttribute>(property.PropertyName.FullName);
            }
        }

        if (PrimitiveTypeChecker.IsTimeSpanType(keySymbol))
        {
            if (!property.HasAttribute<TimeSpanFormatAttribute>())
            {
                throw new AttributeNotFoundException<TimeSpanFormatAttribute>(property.PropertyName.FullName);
            }
        }

        var valueSymbol = (INamedTypeSymbol)property.NamedTypeSymbol.TypeArguments[1];

        if (PrimitiveTypeChecker.IsSupportedPrimitiveType(valueSymbol))
        {
            if (PrimitiveTypeChecker.IsDateTimeType(valueSymbol))
            {
                if (!property.HasAttribute<DateTimeFormatAttribute>())
                {
                    throw new AttributeNotFoundException<DateTimeFormatAttribute>(property.PropertyName.FullName);
                }
            }

            if (PrimitiveTypeChecker.IsTimeSpanType(valueSymbol))
            {
                if (!property.HasAttribute<TimeSpanFormatAttribute>())
                {
                    throw new AttributeNotFoundException<TimeSpanFormatAttribute>(property.PropertyName.FullName);
                }
            }

            return;
        }

        if (valueSymbol.NullableAnnotation is NullableAnnotation.Annotated)
        {
            throw new NotSupportedException("Value type of dictionary must be non-nullable.");
        }

        var valueRecordSchema = RecordTypeChecker.CheckAndGetSchema(valueSymbol, recordSchemaCatalog, visited, logger);

        var valueRecordKeyParameterSchema = valueRecordSchema.RecordPropertySchemata
            .SingleOrDefault(x => x.HasAttribute<KeyAttribute>());

        if (valueRecordKeyParameterSchema is null)
        {
            throw new InvalidUsageException($"{valueRecordSchema.RecordName.FullName} is used as a value in dictionary {property.PropertyName.FullName}, {nameof(KeyAttribute)} must be used in one of the parameters.");
        }

        if (RecordTypeChecker.IsSupportedRecordType(keySymbol))
        {
            var keyRecordSchema = RecordTypeChecker.CheckAndGetSchema(keySymbol, recordSchemaCatalog, visited, logger);

            var valueRecordKeyParameterRecordName = new RecordName(valueRecordKeyParameterSchema.NamedTypeSymbol);
            if (!keyRecordSchema.RecordName.Equals(valueRecordKeyParameterRecordName))
            {
                throw new NotSupportedException($"Key and value type of dictionary must be same type.");
            }

            return;
        }

        CheckSamePrimitiveType(keySymbol, valueRecordKeyParameterSchema.NamedTypeSymbol);
    }

    public static bool IsSupportedDictionaryType(INamedTypeSymbol symbol)
    {
        if (symbol.OriginalDefinition.SpecialType is SpecialType.System_Nullable_T ||
            symbol.TypeArguments is not [INamedTypeSymbol, INamedTypeSymbol])
        {
            return false;
        }

        var genericTypeDefinitionName = symbol
            .ConstructUnboundGenericType()
            .ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        return SupportedTypeNames.Contains(genericTypeDefinitionName);
    }

    public static bool IsPrimitiveKeyAndValueDictionaryType(INamedTypeSymbol symbol)
    {
        if (!IsSupportedDictionaryType(symbol))
        {
            return false;
        }

        var keySymbol = (INamedTypeSymbol)symbol.TypeArguments[0];
        if (keySymbol.NullableAnnotation is NullableAnnotation.Annotated)
        {
            throw new NotSupportedException($"Key type of dictionary must be non-nullable.");
        }

        var valueSymbol = (INamedTypeSymbol)symbol.TypeArguments[1];

        return PrimitiveTypeChecker.IsSupportedPrimitiveType(keySymbol) &&
               PrimitiveTypeChecker.IsSupportedPrimitiveType(valueSymbol);
    }

    public static bool IsPrimitiveKeyRecordValueDictionaryType(INamedTypeSymbol symbol)
    {
        if (!IsSupportedDictionaryType(symbol))
        {
            return false;
        }

        var keySymbol = (INamedTypeSymbol)symbol.TypeArguments[0];
        if (keySymbol.NullableAnnotation is NullableAnnotation.Annotated)
        {
            throw new NotSupportedException($"Key type of dictionary must be non-nullable.");
        }

        var valueSymbol = (INamedTypeSymbol)symbol.TypeArguments[1];
        if (valueSymbol.NullableAnnotation is NullableAnnotation.Annotated)
        {
            throw new NotSupportedException($"Value type of dictionary must be non-nullable.");
        }

        return PrimitiveTypeChecker.IsSupportedPrimitiveType(keySymbol) &&
               RecordTypeChecker.IsSupportedRecordType(valueSymbol);
    }

    public static bool IsRecordKeyAndValueDictionaryType(INamedTypeSymbol symbol)
    {
        if (!IsSupportedDictionaryType(symbol))
        {
            return false;
        }

        var keySymbol = (INamedTypeSymbol)symbol.TypeArguments[0];
        if (keySymbol.NullableAnnotation is NullableAnnotation.Annotated)
        {
            throw new NotSupportedException($"Key type of dictionary must be non-nullable.");
        }

        var valueSymbol = (INamedTypeSymbol)symbol.TypeArguments[1];
        if (valueSymbol.NullableAnnotation is NullableAnnotation.Annotated)
        {
            throw new NotSupportedException($"Value type of dictionary must be non-nullable.");
        }

        return RecordTypeChecker.IsSupportedRecordType(keySymbol) &&
               RecordTypeChecker.IsSupportedRecordType(valueSymbol);
    }

    private static void CheckUnavailableAttribute(PropertySchemaBase property)
    {
        if (property.HasAttribute<ForeignKeyAttribute>())
        {
            throw new InvalidUsageException($"{nameof(ForeignKeyAttribute)} is not available for dictionary type {property.PropertyName.FullName}.");
        }

        if (property.HasAttribute<KeyAttribute>())
        {
            throw new InvalidUsageException($"{nameof(KeyAttribute)} is not available for dictionary type {property.PropertyName.FullName}.");
        }

        if (property.HasAttribute<NullStringAttribute>())
        {
            throw new InvalidUsageException($"{nameof(NullStringAttribute)} is not available for dictionary type {property.PropertyName.FullName}.");
        }

        if (property.HasAttribute<SingleColumnCollectionAttribute>())
        {
            throw new InvalidUsageException($"{nameof(NullStringAttribute)} is not available for dictionary type {property.PropertyName.FullName}.");
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
            throw new NotSupportedException($"Key and value type of dictionary must be same type.");
        }

        if (keySymbol.Name != valueSymbol.Name)
        {
            throw new NotSupportedException($"Key and value type of dictionary must be same type.");
        }
    }
}
