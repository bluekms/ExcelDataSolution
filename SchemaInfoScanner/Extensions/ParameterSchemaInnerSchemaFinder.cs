using Microsoft.CodeAnalysis;
using SchemaInfoScanner.Catalogs;
using SchemaInfoScanner.Schemata;
using SchemaInfoScanner.TypeCheckers;

namespace SchemaInfoScanner.Extensions;

public static class ParameterSchemaInnerSchemaFinder
{
    public static RecordSchema FindInnerRecordSchema(
        this PropertySchemaBase property,
        RecordSchemaCatalog recordSchemaCatalog)
    {
        var typeArgument = GetTypeArgument(property);
        var typeArgumentSchema = recordSchemaCatalog.TryFind(typeArgument);
        if (typeArgumentSchema is null)
        {
            var innerException = new KeyNotFoundException($"{typeArgument.Name} is not found in the RecordSchemaDictionary");
            throw new NotSupportedException($"{property.PropertyName.FullName} is not supported type.", innerException);
        }

        return typeArgumentSchema;
    }

    private static INamedTypeSymbol GetTypeArgument(PropertySchemaBase property)
    {
        if (ArrayTypeChecker.IsSupportedArrayType(property.NamedTypeSymbol) ||
            SetTypeChecker.IsSupportedSetType(property.NamedTypeSymbol))
        {
            return (INamedTypeSymbol)property.NamedTypeSymbol.TypeArguments.Single();
        }
        else if (MapTypeChecker.IsSupportedMapType(property.NamedTypeSymbol))
        {
            return (INamedTypeSymbol)property.NamedTypeSymbol.TypeArguments.Last();
        }

        throw new InvalidOperationException($"Expected {property.PropertyName.FullName} to be record collection type.");
    }
}
