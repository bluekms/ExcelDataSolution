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

public static class PrimitiveHashSetPropertySchemaFactory
{
    public static PropertySchemaBase Create(
        PropertyName propertyName,
        INamedTypeSymbol propertySymbol,
        IReadOnlyList<AttributeSyntax> attributeList)
    {
        if (!SetTypeChecker.IsPrimitiveSetType(propertySymbol))
        {
            throw new NotSupportedException($"{propertyName}({propertySymbol.Name}) is not a supported hash set type.");
        }

        var typeArgumentSymbol = (INamedTypeSymbol)propertySymbol.TypeArguments.Single();
        var isNullable = typeArgumentSymbol.OriginalDefinition.SpecialType is SpecialType.System_Nullable_T;

        var nestedSchema = PrimitivePropertySchemaFactory.Create(
            propertyName,
            typeArgumentSymbol,
            attributeList);

        PrimitiveTypeChecker.Check(nestedSchema);

        var genericArgumentSchema = new PrimitiveTypeGenericArgumentSchema(
            PrimitiveTypeGenericArgumentSchema.CollectionKind.HashSet,
            nestedSchema);

        return isNullable
            ? new NullablePrimitiveHashSetPropertySchema(
                genericArgumentSchema,
                propertySymbol,
                attributeList)
            : new PrimitiveHashSetPropertySchema(
                genericArgumentSchema,
                propertySymbol,
                attributeList);
    }

    public static PropertySchemaBase CreateForSingleColumn(
        PropertyName propertyName,
        INamedTypeSymbol propertySymbol,
        IReadOnlyList<AttributeSyntax> attributeList)
    {
        if (!SetTypeChecker.IsPrimitiveSetType(propertySymbol))
        {
            throw new NotSupportedException($"{propertyName}({propertySymbol.Name}) is not a supported hash set type.");
        }

        if (!AttributeAccessors.TryGetAttributeValue<SingleColumnCollectionAttribute, string>(
                attributeList,
                out var separator))
        {
            throw new InvalidOperationException($"{propertyName}({propertySymbol.Name}) is not a single column hash set.");
        }

        var typeArgumentSymbol = (INamedTypeSymbol)propertySymbol.TypeArguments.Single();
        var isNullable = typeArgumentSymbol.OriginalDefinition.SpecialType is SpecialType.System_Nullable_T;

        var nestedSchema = PrimitivePropertySchemaFactory.Create(
            propertyName,
            typeArgumentSymbol,
            attributeList);

        PrimitiveTypeChecker.Check(nestedSchema);

        var genericArgumentSchema = new PrimitiveTypeGenericArgumentSchema(
            PrimitiveTypeGenericArgumentSchema.CollectionKind.HashSet,
            nestedSchema);

        return isNullable
            ? new SingleColumnNullablePrimitiveHashSetPropertySchema(
                genericArgumentSchema,
                propertySymbol,
                attributeList,
                separator)
            : new SingleColumnPrimitiveHashSetPropertySchema(
                genericArgumentSchema,
                propertySymbol,
                attributeList,
                separator);
    }
}
