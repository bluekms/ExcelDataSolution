using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.Extensions;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.Schemata.TypedPropertySchemata.ContainerTypes;
using SchemaInfoScanner.TypeCheckers;
using StaticDataAttribute;

namespace SchemaInfoScanner.Schemata.TypedPropertySchemaFactories;

public static class PrimitiveListPropertySchemaFactory
{
    public static PropertySchemaBase Create(
        PropertyName propertyName,
        INamedTypeSymbol propertySymbol,
        IReadOnlyList<AttributeSyntax> attributeList)
    {
        if (!ListTypeChecker.IsPrimitiveListType(propertySymbol))
        {
            throw new NotSupportedException($"{propertyName} is not a supported list type.");
        }

        var innerSymbol = (INamedTypeSymbol)propertySymbol.TypeArguments.Single();
        var nestedSchema = PrimitivePropertySchemaFactory.Create(
            propertyName,
            innerSymbol,
            attributeList);

        var genericArgumentSchema = new PrimitiveTypeGenericArgumentSchema(
            PrimitiveTypeGenericArgumentSchema.ContainerKind.List,
            nestedSchema);

        return new PrimitiveListPropertySchema(
            genericArgumentSchema,
            propertySymbol,
            attributeList);
    }

    public static PropertySchemaBase CreateForSingleColumn(
        PropertyName propertyName,
        INamedTypeSymbol propertySymbol,
        IReadOnlyList<AttributeSyntax> attributeList)
    {
        if (!ListTypeChecker.IsPrimitiveListType(propertySymbol))
        {
            throw new NotSupportedException($"{propertyName} is not a supported list type.");
        }

        if (AttributeAccessors.TryGetAttributeValue<SingleColumnContainerAttribute, string>(
                attributeList,
                out var separator))
        {
            throw new InvalidOperationException($"{propertyName} is not a single column list.");
        }

        var innerSymbol = (INamedTypeSymbol)propertySymbol.TypeArguments.Single();
        var nestedSchema = PrimitivePropertySchemaFactory.Create(
            propertyName,
            innerSymbol,
            attributeList);

        var genericArgumentSchema = new PrimitiveTypeGenericArgumentSchema(
            PrimitiveTypeGenericArgumentSchema.ContainerKind.List,
            nestedSchema);

        return new SingleColumnPrimitiveListPropertySchema(
            genericArgumentSchema,
            propertySymbol,
            attributeList,
            separator);
    }
}
