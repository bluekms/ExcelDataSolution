using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.Schemata.TypedPropertySchemata.RecordTypes;
using SchemaInfoScanner.TypeCheckers;

namespace SchemaInfoScanner.Schemata.TypedPropertySchemaFactories.RecordTypes;

public static class RecordListPropertySchemaFactory
{
    public static PropertySchemaBase Create(
        PropertyName propertyName,
        INamedTypeSymbol propertySymbol,
        IReadOnlyList<AttributeSyntax> attributeList,
        INamedTypeSymbol parentRecordSymbol)
    {
        if (!ListTypeChecker.IsSupportedListType(propertySymbol))
        {
            throw new InvalidOperationException($"{propertyName}({propertySymbol.Name}) is not a supported list type.");
        }

        if (ListTypeChecker.IsPrimitiveListType(propertySymbol))
        {
            throw new InvalidOperationException($"{propertyName}({propertySymbol.Name}) is record list type.");
        }

        var typeArgumentSymbol = (INamedTypeSymbol)propertySymbol.TypeArguments.Single();
        if (CollectionTypeChecker.IsSupportedCollectionType(typeArgumentSymbol))
        {
            throw new NotSupportedException($"{propertyName}({typeArgumentSymbol.Name}) is not a supported nested collection type.");
        }

        var nestedSchema = RecordPropertySchemaFactory.Create(
            propertyName,
            typeArgumentSymbol,
            attributeList,
            parentRecordSymbol);

        var genericArgumentSchema = new RecordTypeGenericArgumentSchema(
            RecordTypeGenericArgumentSchema.CollectionKind.List,
            nestedSchema);

        return new RecordListPropertySchema(
            genericArgumentSchema,
            propertySymbol,
            attributeList);
    }
}
