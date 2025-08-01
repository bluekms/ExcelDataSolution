using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner.Catalogs;
using SchemaInfoScanner.Exceptions;
using SchemaInfoScanner.Extensions;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.Schemata;
using StaticDataAttribute;

namespace SchemaInfoScanner.TypeCheckers;

internal static class ArrayTypeChecker
{
    private static readonly HashSet<string> SupportedTypeNames =
    [
        "ImmutableArray<>",
    ];

    public static void Check(
        PropertySchemaBase property,
        RecordSchemaCatalog recordSchemaCatalog,
        HashSet<RecordName> visited,
        ILogger logger)
    {
        if (!IsSupportedArrayType(property.NamedTypeSymbol))
        {
            throw new InvalidOperationException($"Expected {property.PropertyName.FullName} to be supported array type, but actually not supported.");
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
            throw new NotSupportedException($"{property.PropertyName.FullName} is not supported array type. Nullable record item for array is not supported.");
        }

        if (property.HasAttribute<SingleColumnCollectionAttribute>())
        {
            throw new NotSupportedException($"{property.PropertyName.FullName} is not supported array type. {nameof(SingleColumnCollectionAttribute)} can only be used in primitive type array.");
        }

        var innerRecordSchema = property.FindInnerRecordSchema(recordSchemaCatalog);
        RecordTypeChecker.Check(innerRecordSchema, recordSchemaCatalog, visited, logger);
    }

    private static void CheckUnavailableAttribute(PropertySchemaBase property)
    {
        if (property.HasAttribute<ForeignKeyAttribute>())
        {
            throw new InvalidUsageException($"{nameof(ForeignKeyAttribute)} is not available for array type {property.PropertyName.FullName}.");
        }

        if (property.HasAttribute<KeyAttribute>())
        {
            throw new InvalidUsageException($"{nameof(KeyAttribute)} is not available for array type {property.PropertyName.FullName}.");
        }
    }

    public static bool IsSupportedArrayType(INamedTypeSymbol symbol)
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

    public static bool IsPrimitiveArrayType(INamedTypeSymbol symbol)
    {
        if (!IsSupportedArrayType(symbol))
        {
            return false;
        }

        var typeArgument = (INamedTypeSymbol)symbol.TypeArguments.Single();
        return PrimitiveTypeChecker.IsSupportedPrimitiveType(typeArgument);
    }
}
