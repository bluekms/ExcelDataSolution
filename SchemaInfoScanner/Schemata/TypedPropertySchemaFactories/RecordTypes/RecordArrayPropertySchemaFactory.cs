using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.Schemata.TypedPropertySchemata.RecordTypes;
using SchemaInfoScanner.TypeCheckers;

namespace SchemaInfoScanner.Schemata.TypedPropertySchemaFactories.RecordTypes;

public static class RecordArrayPropertySchemaFactory
{
    public static PropertySchemaBase Create(
        PropertyName propertyName,
        INamedTypeSymbol propertySymbol,
        IReadOnlyList<AttributeSyntax> attributeList,
        INamedTypeSymbol parentRecordSymbol)
    {
        if (!ArrayTypeChecker.IsSupportedArrayType(propertySymbol))
        {
            throw new InvalidOperationException($"{propertyName}({propertySymbol.Name}) is not a supported array type.");
        }

        if (ArrayTypeChecker.IsPrimitiveArrayType(propertySymbol))
        {
            throw new InvalidOperationException($"{propertyName}({propertySymbol.Name}) is record array type.");
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
            RecordTypeGenericArgumentSchema.CollectionKind.Array,
            nestedSchema);

        return new RecordArrayPropertySchema(
            genericArgumentSchema,
            propertySymbol,
            attributeList);
    }
}
