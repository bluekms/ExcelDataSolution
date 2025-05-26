using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.Extensions;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.Schemata.TypedPropertySchemata.ContainerTypes;
using SchemaInfoScanner.Schemata.TypedPropertySchemata.RecordTypes;
using SchemaInfoScanner.TypeCheckers;
using StaticDataAttribute;

namespace SchemaInfoScanner.Schemata.TypedPropertySchemaFactories;

public static class PrimitiveKeyAndValueDictionarySchemaFactory
{
    public static PropertySchemaBase Create(
        PropertyName propertyName,
        INamedTypeSymbol propertySymbol,
        IReadOnlyList<AttributeSyntax> attributeList)
    {
        var keySchema = new PrimitiveTypeGenericArgumentSchema(
            PrimitiveTypeGenericArgumentSchema.ContainerKind.DictionaryKey,
            PrimitivePropertySchemaFactory.Create(
                propertyName,
                (INamedTypeSymbol)propertySymbol.TypeArguments[0],
                attributeList));

        var valueSchema = new PrimitiveTypeGenericArgumentSchema(
            PrimitiveTypeGenericArgumentSchema.ContainerKind.DictionaryValue,
            PrimitivePropertySchemaFactory.Create(
                propertyName,
                (INamedTypeSymbol)propertySymbol.TypeArguments[1],
                attributeList));

        return new PrimitiveKeyPrimitiveValueDictionaryPropertySchema(
            propertyName,
            propertySymbol,
            attributeList,
            keySchema,
            valueSchema);
    }
}
