using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.Extensions;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.Schemata.TypedPropertySchemaFactories.CatalogTypes;
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
        else if (ListTypeChecker.IsPrimitiveListType(propertySymbol))
        {
            var isSingleColumnCatalog = AttributeAccessors.HasAttribute<SingleColumnCatalogAttribute>(attributeList);
            return isSingleColumnCatalog
                ? PrimitiveListPropertySchemaFactory.CreateForSingleColumn(propertyName, propertySymbol, attributeList)
                : PrimitiveListPropertySchemaFactory.Create(propertyName, propertySymbol, attributeList);
        }
        else if (HashSetTypeChecker.IsPrimitiveHashSetType(propertySymbol))
        {
            var isSingleColumnCatalog = AttributeAccessors.HasAttribute<SingleColumnCatalogAttribute>(attributeList);
            return isSingleColumnCatalog
                ? PrimitiveHashSetPropertySchemaFactory.CreateForSingleColumn(propertyName, propertySymbol, attributeList)
                : PrimitiveHashSetPropertySchemaFactory.Create(propertyName, propertySymbol, attributeList);
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
            return Create(propertyName, nestedRecordSymbol, attributeList, parentRecordSymbol);
        }
        else if (DictionaryTypeChecker.IsPrimitiveKeyAndValueDictionaryType(propertySymbol))
        {
            return PrimitiveKeyAndValueDictionarySchemaFactory.Create(
                propertyName,
                propertySymbol,
                attributeList);
        }
        else if (ListTypeChecker.IsSupportedListType(propertySymbol))
        {
            return RecordListPropertySchemaFactory.Create(
                propertyName,
                propertySymbol,
                attributeList,
                parentRecordSymbol);
        }
        else if (HashSetTypeChecker.IsSupportedHashSetType(propertySymbol))
        {
            return RecordHashSetPropertySchemaFactory.Create(
                propertyName,
                propertySymbol,
                attributeList,
                parentRecordSymbol);
        }
        else if (DictionaryTypeChecker.IsPrimitiveKeyRecordValueDictionaryType(propertySymbol))
        {
            return PrimitiveKeyRecordValueDictionaryPropertySchemaFactory.Create(
                propertyName,
                propertySymbol,
                attributeList,
                parentRecordSymbol);
        }
        else if (DictionaryTypeChecker.IsRecordKeyAndValueDictionaryType(propertySymbol))
        {
            return RecordKeyAndValueDictionaryPropertySchemaFactory.Create(
                propertyName,
                propertySymbol,
                attributeList,
                parentRecordSymbol);
        }

        throw new NotSupportedException($"{propertyName}: {propertySymbol}");
    }
}
