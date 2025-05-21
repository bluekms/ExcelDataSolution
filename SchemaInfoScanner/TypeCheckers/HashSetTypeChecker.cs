using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.Exceptions;
using SchemaInfoScanner.Extensions;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.Schemata;
using StaticDataAttribute;

namespace SchemaInfoScanner.TypeCheckers;

internal static class HashSetTypeChecker
{
    private static readonly HashSet<string> SupportedTypeNames =
    [
        "HashSet<>",
        "IReadOnlyCollection<>",
    ];

    public static void Check(
        PropertySchemaBase property,
        RecordSchemaContainer recordSchemaContainer,
        HashSet<RecordName> visited,
        ILogger logger)
    {
        if (!IsSupportedHashSetType(property.NamedTypeSymbol))
        {
            throw new InvalidOperationException($"Expected {property.ParameterName.FullName} to be supported hashset type, but actually not supported.");
        }

        CheckUnavailableAttribute(property);

        var typeArgument = (INamedTypeSymbol)property.NamedTypeSymbol.TypeArguments.Single();
        if (PrimitiveTypeChecker.IsSupportedPrimitiveType(typeArgument))
        {
            return;
        }

        if (typeArgument.NullableAnnotation is NullableAnnotation.Annotated)
        {
            throw new TypeNotSupportedException($"{property.ParameterName.FullName} is not supported hashset type. Nullable record item for hashset is not supported.");
        }

        if (property.HasAttribute<SingleColumnContainerAttribute>())
        {
            throw new TypeNotSupportedException($"{property.ParameterName.FullName} is not supported hashset type. {nameof(SingleColumnContainerAttribute)} can only be used in primitive type hashset.");
        }

        var innerRecordSchema = property.FindInnerRecordSchema(recordSchemaContainer);
        RecordTypeChecker.Check(innerRecordSchema, recordSchemaContainer, visited, logger);
    }

    private static void CheckUnavailableAttribute(PropertySchemaBase property)
    {
        if (property.HasAttribute<ForeignKeyAttribute>())
        {
            throw new InvalidUsageException($"{nameof(ForeignKeyAttribute)} is not available for hashset type {property.ParameterName.FullName}.");
        }

        if (property.HasAttribute<KeyAttribute>())
        {
            throw new InvalidUsageException($"{nameof(KeyAttribute)} is not available for hashset type {property.ParameterName.FullName}.");
        }

        if (property.HasAttribute<NullStringAttribute>())
        {
            throw new InvalidUsageException($"{nameof(NullStringAttribute)} is not available for hashset type {property.ParameterName.FullName}.");
        }
    }

    public static bool IsSupportedHashSetType(INamedTypeSymbol symbol)
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
}
