using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner.Containers;
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
        "ImmutableDictionary<, >",
        "ImmutableSortedDictionary<, >",
        "FrozenDictionary<, >"
    ];

    public static void Check(
        RawParameterSchema rawParameter,
        RecordSchemaContainer recordSchemaContainer,
        HashSet<RecordName> visited,
        ILogger logger)
    {
        if (!IsSupportedDictionaryType(rawParameter.NamedTypeSymbol))
        {
            throw new InvalidOperationException($"Expected {rawParameter.ParameterName.FullName} to be supported dictionary type, but actually not supported.");
        }

        CheckUnavailableAttribute(rawParameter);

        var keySymbol = (INamedTypeSymbol)rawParameter.NamedTypeSymbol.TypeArguments[0];
        if (keySymbol.NullableAnnotation is NullableAnnotation.Annotated)
        {
            throw new TypeNotSupportedException($"Key type of dictionary must be non-nullable.");
        }

        var valueSymbol = (INamedTypeSymbol)rawParameter.NamedTypeSymbol.TypeArguments[1];
        if (valueSymbol.NullableAnnotation is NullableAnnotation.Annotated)
        {
            throw new TypeNotSupportedException($"Value type of dictionary must be non-nullable.");
        }

        var valueRecordSchema = RecordTypeChecker.CheckAndGetSchema(valueSymbol, recordSchemaContainer, visited, logger);

        var valueRecordKeyParameterSchema = valueRecordSchema.RawParameterSchemaList
            .SingleOrDefault(x => x.HasAttribute<KeyAttribute>());

        if (valueRecordKeyParameterSchema is null)
        {
            throw new InvalidUsageException($"{valueRecordSchema.RecordName.FullName} is used as a value in dictionary {rawParameter.ParameterName.FullName}, KeyAttribute must be used in one of the parameters.");
        }

        if (RecordTypeChecker.IsSupportedRecordType(keySymbol))
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

    private static void CheckUnavailableAttribute(RawParameterSchema rawParameter)
    {
        if (rawParameter.HasAttribute<ForeignKeyAttribute>())
        {
            throw new InvalidUsageException($"{nameof(ForeignKeyAttribute)} is not available for dictionary type {rawParameter.ParameterName.FullName}.");
        }

        if (rawParameter.HasAttribute<KeyAttribute>())
        {
            throw new InvalidUsageException($"{nameof(KeyAttribute)} is not available for dictionary type {rawParameter.ParameterName.FullName}.");
        }

        if (rawParameter.HasAttribute<NullStringAttribute>())
        {
            throw new InvalidUsageException($"{nameof(NullStringAttribute)} is not available for dictionary type {rawParameter.ParameterName.FullName}.");
        }

        if (rawParameter.HasAttribute<SingleColumnContainerAttribute>())
        {
            throw new InvalidUsageException($"{nameof(NullStringAttribute)} is not available for dictionary type {rawParameter.ParameterName.FullName}.");
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
