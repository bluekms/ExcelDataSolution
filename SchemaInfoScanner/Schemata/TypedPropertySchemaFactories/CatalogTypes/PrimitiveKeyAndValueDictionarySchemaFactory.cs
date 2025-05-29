using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.Exceptions;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.Schemata.TypedPropertySchemaFactories.PrimitiveTypes;
using SchemaInfoScanner.Schemata.TypedPropertySchemata.CatalogTypes;
using SchemaInfoScanner.TypeCheckers;

namespace SchemaInfoScanner.Schemata.TypedPropertySchemaFactories.CatalogTypes;

public static class PrimitiveKeyAndValueDictionarySchemaFactory
{
    public static PropertySchemaBase Create(
        PropertyName propertyName,
        INamedTypeSymbol propertySymbol,
        IReadOnlyList<AttributeSyntax> attributeList)
    {
        var keySymbol = (INamedTypeSymbol)propertySymbol.TypeArguments[0];
        if (!PrimitiveTypeChecker.IsSupportedPrimitiveType(keySymbol))
        {
            throw new TypeNotSupportedException($"{propertyName} Key type of dictionary must be a supported primitive type.");
        }

        var valueSymbol = (INamedTypeSymbol)propertySymbol.TypeArguments[1];
        if (!PrimitiveTypeChecker.IsSupportedPrimitiveType(valueSymbol))
        {
            throw new TypeNotSupportedException($"{propertyName} Value type of dictionary must be a supported primitive type.");
        }

        var keySchema = new PrimitiveTypeGenericArgumentSchema(
            PrimitiveTypeGenericArgumentSchema.CatalogKind.DictionaryKey,
            PrimitivePropertySchemaFactory.Create(propertyName, keySymbol, attributeList));

        var valueSchema = new PrimitiveTypeGenericArgumentSchema(
            PrimitiveTypeGenericArgumentSchema.CatalogKind.DictionaryValue,
            PrimitivePropertySchemaFactory.Create(propertyName, valueSymbol, attributeList));

        return new PrimitiveKeyPrimitiveValueDictionaryPropertySchema(
            propertyName,
            propertySymbol,
            attributeList,
            keySchema,
            valueSchema);
    }
}
