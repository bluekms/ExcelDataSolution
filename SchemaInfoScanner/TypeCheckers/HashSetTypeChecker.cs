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
        ParameterSchemaBase parameter,
        RecordSchemaContainer recordSchemaContainer,
        HashSet<RecordName> visited,
        ILogger logger)
    {
        if (!IsSupportedHashSetType(parameter.NamedTypeSymbol))
        {
            throw new InvalidOperationException($"Expected {parameter.ParameterName.FullName} to be supported hashset type, but actually not supported.");
        }

        CheckUnavailableAttribute(parameter);

        var typeArgument = (INamedTypeSymbol)parameter.NamedTypeSymbol.TypeArguments.Single();
        if (PrimitiveTypeChecker.IsSupportedPrimitiveType(typeArgument))
        {
            return;
        }

        if (typeArgument.NullableAnnotation is NullableAnnotation.Annotated)
        {
            throw new TypeNotSupportedException($"{parameter.ParameterName.FullName} is not supported hashset type. Nullable record item for hashset is not supported.");
        }

        if (parameter.HasAttribute<SingleColumnContainerAttribute>())
        {
            throw new TypeNotSupportedException($"{parameter.ParameterName.FullName} is not supported hashset type. {nameof(SingleColumnContainerAttribute)} can only be used in primitive type hashset.");
        }

        var innerRecordSchema = parameter.FindInnerRecordSchema(recordSchemaContainer);
        RecordTypeChecker.Check(innerRecordSchema, recordSchemaContainer, visited, logger);
    }

    private static void CheckUnavailableAttribute(ParameterSchemaBase parameter)
    {
        if (parameter.HasAttribute<ForeignKeyAttribute>())
        {
            throw new InvalidUsageException($"{nameof(ForeignKeyAttribute)} is not available for hashset type {parameter.ParameterName.FullName}.");
        }

        if (parameter.HasAttribute<KeyAttribute>())
        {
            throw new InvalidUsageException($"{nameof(KeyAttribute)} is not available for hashset type {parameter.ParameterName.FullName}.");
        }

        if (parameter.HasAttribute<NullStringAttribute>())
        {
            throw new InvalidUsageException($"{nameof(NullStringAttribute)} is not available for hashset type {parameter.ParameterName.FullName}.");
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
