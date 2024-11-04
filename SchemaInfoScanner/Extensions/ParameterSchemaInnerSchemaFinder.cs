using Microsoft.CodeAnalysis;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.Exceptions;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.Schemata;
using SchemaInfoScanner.TypeCheckers;

namespace SchemaInfoScanner.Extensions;

public static class ParameterSchemaInnerSchemaFinder
{
    public static RawRecordSchema FindInnerRecordSchema(
        this RawParameterSchema rawParameter,
        RecordSchemaContainer recordSchemaContainer)
    {
        var typeArgument = GetTypeArgument(rawParameter);
        var recordName = new RecordName(typeArgument);
        if (!recordSchemaContainer.RecordSchemaDictionary.TryGetValue(recordName, out var typeArgumentSchema))
        {
            var innerException = new KeyNotFoundException($"{recordName.FullName} is not found in the RecordSchemaDictionary");
            throw new TypeNotSupportedException($"{rawParameter.ParameterName.FullName} is not supported type.", innerException);
        }

        return typeArgumentSchema;
    }

    private static INamedTypeSymbol GetTypeArgument(RawParameterSchema rawParameter)
    {
        if (ListTypeChecker.IsSupportedListType(rawParameter.NamedTypeSymbol) ||
            HashSetTypeChecker.IsSupportedHashSetType(rawParameter.NamedTypeSymbol))
        {
            return (INamedTypeSymbol)rawParameter.NamedTypeSymbol.TypeArguments.Single();
        }
        else if (DictionaryTypeChecker.IsSupportedDictionaryType(rawParameter.NamedTypeSymbol))
        {
            return (INamedTypeSymbol)rawParameter.NamedTypeSymbol.TypeArguments.Last();
        }

        throw new InvalidOperationException($"Expected {rawParameter.ParameterName.FullName} to be record container type.");
    }
}
