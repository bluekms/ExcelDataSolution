using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.Extensions;
using SchemaInfoScanner.NameObjects;
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
            var isSingleColumnContainer = AttributeAccessors.HasAttribute<SingleColumnContainerAttribute>(attributeList);
            return isSingleColumnContainer
                ? PrimitiveListPropertySchemaFactory.CreateForSingleColumn(propertyName, propertySymbol, attributeList)
                : PrimitiveListPropertySchemaFactory.Create(propertyName, propertySymbol, attributeList);
        }
        else if (HashSetTypeChecker.IsPrimitiveHashSetType(propertySymbol))
        {
            var isSingleColumnContainer = AttributeAccessors.HasAttribute<SingleColumnContainerAttribute>(attributeList);
            return isSingleColumnContainer
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
        else if (RecordTypeChecker.TryFindNestedRecordTypeSymbol(
                     parentRecordSymbol,
                     propertySymbol,
                     out var nestedRecordSymbol))
        {
            return Create(propertyName, nestedRecordSymbol, attributeList, parentRecordSymbol);
        }
        else if (DictionaryTypeChecker.IsPrimitiveKeyPrimitiveValueDictionaryType(propertySymbol))
        {
            return PrimitiveKeyAndValueDictionarySchemaFactory.Create(
                propertyName,
                propertySymbol,
                attributeList);
        }

        throw new NotImplementedException();
    }
}
