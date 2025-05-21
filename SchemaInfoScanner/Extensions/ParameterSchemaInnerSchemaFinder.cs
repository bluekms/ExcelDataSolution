using Microsoft.CodeAnalysis;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.Exceptions;
using SchemaInfoScanner.Schemata;
using SchemaInfoScanner.TypeCheckers;

namespace SchemaInfoScanner.Extensions;

public static class ParameterSchemaInnerSchemaFinder
{
    public static RecordSchema FindInnerRecordSchema(
        this PropertySchemaBase property,
        RecordSchemaContainer recordSchemaContainer)
    {
        var typeArgument = GetTypeArgument(property);
        var typeArgumentSchema = recordSchemaContainer.TryFind(typeArgument);
        if (typeArgumentSchema is null)
        {
            var innerException = new KeyNotFoundException($"{typeArgument.Name} is not found in the RecordSchemaDictionary");
            throw new TypeNotSupportedException($"{property.PropertyName.FullName} is not supported type.", innerException);
        }

        return typeArgumentSchema;
    }

    private static INamedTypeSymbol GetTypeArgument(PropertySchemaBase property)
    {
        if (ListTypeChecker.IsSupportedListType(property.NamedTypeSymbol) ||
            HashSetTypeChecker.IsSupportedHashSetType(property.NamedTypeSymbol))
        {
            return (INamedTypeSymbol)property.NamedTypeSymbol.TypeArguments.Single();
        }
        else if (DictionaryTypeChecker.IsSupportedDictionaryType(property.NamedTypeSymbol))
        {
            return (INamedTypeSymbol)property.NamedTypeSymbol.TypeArguments.Last();
        }

        throw new InvalidOperationException($"Expected {property.PropertyName.FullName} to be record container type.");
    }
}
