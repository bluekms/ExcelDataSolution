using Microsoft.CodeAnalysis;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.Exceptions;
using SchemaInfoScanner.Schemata;
using SchemaInfoScanner.TypeCheckers;

namespace SchemaInfoScanner.Extensions;

public static class ParameterSchemaInnerSchemaFinder
{
    public static RecordSchema FindInnerRecordSchema(
        this ParameterSchemaBase parameter,
        RecordSchemaContainer recordSchemaContainer)
    {
        var typeArgument = GetTypeArgument(parameter);
        var typeArgumentSchema = recordSchemaContainer.TryFind(typeArgument);
        if (typeArgumentSchema is null)
        {
            var innerException = new KeyNotFoundException($"{typeArgument.Name} is not found in the RecordSchemaDictionary");
            throw new TypeNotSupportedException($"{parameter.ParameterName.FullName} is not supported type.", innerException);
        }

        return typeArgumentSchema;
    }

    private static INamedTypeSymbol GetTypeArgument(ParameterSchemaBase parameter)
    {
        if (ListTypeChecker.IsSupportedListType(parameter.NamedTypeSymbol) ||
            HashSetTypeChecker.IsSupportedHashSetType(parameter.NamedTypeSymbol))
        {
            return (INamedTypeSymbol)parameter.NamedTypeSymbol.TypeArguments.Single();
        }
        else if (DictionaryTypeChecker.IsSupportedDictionaryType(parameter.NamedTypeSymbol))
        {
            return (INamedTypeSymbol)parameter.NamedTypeSymbol.TypeArguments.Last();
        }

        throw new InvalidOperationException($"Expected {parameter.ParameterName.FullName} to be record container type.");
    }
}
