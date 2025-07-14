using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.Extensions;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.Schemata.TypedPropertySchemaFactories.CollectionTypes;
using SchemaInfoScanner.Schemata.TypedPropertySchemaFactories.PrimitiveTypes;
using SchemaInfoScanner.Schemata.TypedPropertySchemaFactories.RecordTypes;
using SchemaInfoScanner.TypeCheckers;
using StaticDataAttribute;

namespace SchemaInfoScanner.Schemata.TypedPropertySchemaFactories;

public static class TypedPropertySchemaFactory
{
    public static PropertySchemaBase Create(
        PropertyName propertyName,
        INamedTypeSymbol propertySymbol,
        IReadOnlyList<AttributeSyntax> attributeList,
        INamedTypeSymbol parentRecordSymbol)
    {
        if (PrimitiveTypeChecker.IsSupportedPrimitiveType(propertySymbol))
        {
            return PrimitivePropertySchemaFactory.Create(propertyName, propertySymbol, attributeList);
        }
        else if (ArrayTypeChecker.IsPrimitiveArrayType(propertySymbol))
        {
            var isSingleColumnCollection = AttributeAccessors.HasAttribute<SingleColumnCollectionAttribute>(attributeList);
            return isSingleColumnCollection
                ? PrimitiveArrayPropertySchemaFactory.CreateForSingleColumn(propertyName, propertySymbol, attributeList)
                : PrimitiveArrayPropertySchemaFactory.Create(propertyName, propertySymbol, attributeList);
        }
        else if (SetTypeChecker.IsPrimitiveSetType(propertySymbol))
        {
            var isSingleColumnCollection = AttributeAccessors.HasAttribute<SingleColumnCollectionAttribute>(attributeList);
            return isSingleColumnCollection
                ? PrimitiveHashSetPropertySchemaFactory.CreateForSingleColumn(propertyName, propertySymbol, attributeList)
                : PrimitiveHashSetPropertySchemaFactory.Create(propertyName, propertySymbol, attributeList);
        }
        else if (MapTypeChecker.IsPrimitiveKeyAndValueMapType(propertySymbol))
        {
            return PrimitiveKeyAndValueDictionarySchemaFactory.Create(
                propertyName,
                propertySymbol,
                attributeList);
        }
        else if (RecordTypeChecker.IsSupportedRecordType(propertySymbol))
        {
            return RecordPropertySchemaFactory.Create(
                propertyName,
                propertySymbol,
                attributeList,
                parentRecordSymbol);
        }
        else if (RecordTypeChecker.TryFindNestedRecordTypeSymbol(parentRecordSymbol, propertySymbol, out var nestedRecordSymbol))
        {
            return RecordPropertySchemaFactory.Create(
                propertyName,
                nestedRecordSymbol,
                attributeList,
                parentRecordSymbol);
        }
        else if (ArrayTypeChecker.IsSupportedArrayType(propertySymbol))
        {
            return RecordArrayPropertySchemaFactory.Create(
                propertyName,
                propertySymbol,
                attributeList,
                parentRecordSymbol);
        }
        else if (SetTypeChecker.IsSupportedSetType(propertySymbol))
        {
            return RecordHashSetPropertySchemaFactory.Create(
                propertyName,
                propertySymbol,
                attributeList,
                parentRecordSymbol);
        }
        else if (MapTypeChecker.IsPrimitiveKeyRecordValueMapType(propertySymbol))
        {
            return PrimitiveKeyRecordValueDictionaryPropertySchemaFactory.Create(
                propertyName,
                propertySymbol,
                attributeList,
                parentRecordSymbol);
        }
        else if (MapTypeChecker.IsRecordKeyAndValueMapType(propertySymbol))
        {
            return RecordKeyAndValueDictionaryPropertySchemaFactory.Create(
                propertyName,
                propertySymbol,
                attributeList,
                parentRecordSymbol);
        }

        throw new NotSupportedException($"{propertyName}({propertySymbol.Name}) is not a supported property type.");
    }
}
