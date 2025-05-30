using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.Schemata.TypedPropertySchemaFactories.PrimitiveTypes;
using SchemaInfoScanner.Schemata.TypedPropertySchemata.CollectionTypes;
using SchemaInfoScanner.TypeCheckers;

namespace SchemaInfoScanner.Schemata.TypedPropertySchemaFactories.CollectionTypes;

public static class PrimitiveKeyAndValueDictionarySchemaFactory
{
    public static PropertySchemaBase Create(
        PropertyName propertyName,
        INamedTypeSymbol propertySymbol,
        IReadOnlyList<AttributeSyntax> attributeList)
    {
        if (!DictionaryTypeChecker.IsSupportedDictionaryType(propertySymbol))
        {
            throw new NotSupportedException($"{propertyName}({propertySymbol.Name}) is not a supported dictionary type.");
        }

        var keySymbol = (INamedTypeSymbol)propertySymbol.TypeArguments[0];
        if (!PrimitiveTypeChecker.IsSupportedPrimitiveType(keySymbol))
        {
            throw new NotSupportedException($"{propertyName}({propertySymbol.Name}) Key type of dictionary must be a supported primitive type.");
        }

        var valueSymbol = (INamedTypeSymbol)propertySymbol.TypeArguments[1];
        if (!PrimitiveTypeChecker.IsSupportedPrimitiveType(valueSymbol))
        {
            throw new NotSupportedException($"{propertyName}({propertySymbol.Name}) Value type of dictionary must be a supported primitive type.");
        }

        var keySchema = new PrimitiveTypeGenericArgumentSchema(
            PrimitiveTypeGenericArgumentSchema.CollectionKind.DictionaryKey,
            PrimitivePropertySchemaFactory.Create(propertyName, keySymbol, attributeList));

        var valueSchema = new PrimitiveTypeGenericArgumentSchema(
            PrimitiveTypeGenericArgumentSchema.CollectionKind.DictionaryValue,
            PrimitivePropertySchemaFactory.Create(propertyName, valueSymbol, attributeList));

        return new PrimitiveKeyPrimitiveValueDictionaryPropertySchema(
            propertyName,
            propertySymbol,
            attributeList,
            keySchema,
            valueSchema);
    }
}
