using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.Extensions;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.Schemata.TypedPropertySchemaFactories.PrimitiveTypes;
using SchemaInfoScanner.Schemata.TypedPropertySchemata.CollectionTypes;
using SchemaInfoScanner.Schemata.TypedPropertySchemata.CollectionTypes.NullableTypes;
using SchemaInfoScanner.TypeCheckers;
using StaticDataAttribute;

namespace SchemaInfoScanner.Schemata.TypedPropertySchemaFactories.CollectionTypes;

public static class PrimitiveListPropertySchemaFactory
{
    public static PropertySchemaBase Create(
        PropertyName propertyName,
        INamedTypeSymbol propertySymbol,
        IReadOnlyList<AttributeSyntax> attributeList)
    {
        if (!ListTypeChecker.IsPrimitiveListType(propertySymbol))
        {
            throw new NotSupportedException($"{propertyName}({propertySymbol.Name}) is not a supported list type.");
        }

        var typeArgumentSymbol = (INamedTypeSymbol)propertySymbol.TypeArguments.Single();
        var isNullable = typeArgumentSymbol.OriginalDefinition.SpecialType is SpecialType.System_Nullable_T;

        var nestedSchema = PrimitivePropertySchemaFactory.Create(
            propertyName,
            typeArgumentSymbol,
            attributeList);

        PrimitiveTypeChecker.Check(nestedSchema);

        var genericArgumentSchema = new PrimitiveTypeGenericArgumentSchema(
            PrimitiveTypeGenericArgumentSchema.CollectionKind.List,
            nestedSchema);

        return isNullable
            ? new NullablePrimitiveListPropertySchema(
                genericArgumentSchema,
                propertySymbol,
                attributeList)
            : new PrimitiveListPropertySchema(
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
            throw new NotSupportedException($"{propertyName}({propertySymbol.Name}) is not a supported list type.");
        }

        if (!AttributeAccessors.TryGetAttributeValue<SingleColumnCollectionAttribute, string>(
            attributeList,
            out var separator))
        {
            throw new InvalidOperationException($"{propertyName}({propertySymbol.Name}) is not a single column list.");
        }

        var typeArgumentSymbol = (INamedTypeSymbol)propertySymbol.TypeArguments.Single();
        var isNullable = typeArgumentSymbol.OriginalDefinition.SpecialType is SpecialType.System_Nullable_T;

        var nestedSchema = PrimitivePropertySchemaFactory.Create(
            propertyName,
            typeArgumentSymbol,
            attributeList);

        PrimitiveTypeChecker.Check(nestedSchema);

        var genericArgumentSchema = new PrimitiveTypeGenericArgumentSchema(
            PrimitiveTypeGenericArgumentSchema.CollectionKind.List,
            nestedSchema);

        return isNullable
            ? new SingleColumnNullablePrimitiveListPropertySchema(
                genericArgumentSchema,
                propertySymbol,
                attributeList,
                separator)
            : new SingleColumnPrimitiveListPropertySchema(
                genericArgumentSchema,
                propertySymbol,
                attributeList,
                separator);
    }
}
