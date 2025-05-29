using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner.Catalogs;
using SchemaInfoScanner.Exceptions;
using SchemaInfoScanner.Extensions;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.Schemata;
using StaticDataAttribute;

namespace SchemaInfoScanner.TypeCheckers;

internal static class ListTypeChecker
{
    private static readonly HashSet<string> SupportedTypeNames =
    [
        "List<>",
        "IReadOnlyList<>",
    ];

    public static void Check(
        PropertySchemaBase property,
        RecordSchemaCatalog recordSchemaCatalog,
        HashSet<RecordName> visited,
        ILogger logger)
    {
        if (!IsSupportedListType(property.NamedTypeSymbol))
        {
            throw new InvalidOperationException($"Expected {property.PropertyName.FullName} to be supported list type, but actually not supported.");
        }

        CheckUnavailableAttribute(property);

        var typeArgument = (INamedTypeSymbol)property.NamedTypeSymbol.TypeArguments.Single();

        if (PrimitiveTypeChecker.IsDateTimeType(typeArgument))
        {
            if (!property.HasAttribute<DateTimeFormatAttribute>())
            {
                throw new AttributeNotFoundException<DateTimeFormatAttribute>(property.PropertyName.FullName);
            }
        }

        if (PrimitiveTypeChecker.IsTimeSpanType(typeArgument))
        {
            if (!property.HasAttribute<TimeSpanFormatAttribute>())
            {
                throw new AttributeNotFoundException<TimeSpanFormatAttribute>(property.PropertyName.FullName);
            }
        }

        if (PrimitiveTypeChecker.IsSupportedPrimitiveType(typeArgument))
        {
            return;
        }

        if (typeArgument.NullableAnnotation is NullableAnnotation.Annotated)
        {
            throw new NotSupportedException($"{property.PropertyName.FullName} is not supported list type. Nullable record item for list is not supported.");
        }

        if (property.HasAttribute<SingleColumnCollectionAttribute>())
        {
            throw new NotSupportedException($"{property.PropertyName.FullName} is not supported list type. {nameof(SingleColumnCollectionAttribute)} can only be used in primitive type list.");
        }

        var innerRecordSchema = property.FindInnerRecordSchema(recordSchemaCatalog);
        RecordTypeChecker.Check(innerRecordSchema, recordSchemaCatalog, visited, logger);
    }

    private static void CheckUnavailableAttribute(PropertySchemaBase property)
    {
        if (property.HasAttribute<ForeignKeyAttribute>())
        {
            throw new InvalidUsageException($"{nameof(ForeignKeyAttribute)} is not available for list type {property.PropertyName.FullName}.");
        }

        if (property.HasAttribute<KeyAttribute>())
        {
            throw new InvalidUsageException($"{nameof(KeyAttribute)} is not available for list type {property.PropertyName.FullName}.");
        }

        if (property.HasAttribute<NullStringAttribute>())
        {
            throw new InvalidUsageException($"{nameof(NullStringAttribute)} is not available for list type {property.PropertyName.FullName}.");
        }
    }

    public static bool IsSupportedListType(INamedTypeSymbol symbol)
    {
        if (symbol.OriginalDefinition.SpecialType is SpecialType.System_Nullable_T ||
            symbol.TypeArguments is not [INamedTypeSymbol])
        {
            return false;
        }

        var genericTypeDefinitionName = symbol
            .ConstructUnboundGenericType()
            .ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        return SupportedTypeNames.Contains(genericTypeDefinitionName);
    }

    public static bool IsPrimitiveListType(INamedTypeSymbol symbol)
    {
        if (!IsSupportedListType(symbol))
        {
            return false;
        }

        var typeArgument = (INamedTypeSymbol)symbol.TypeArguments.Single();
        return PrimitiveTypeChecker.IsSupportedPrimitiveType(typeArgument);
    }
}
