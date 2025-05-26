using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.Exceptions;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.Schemata.TypedPropertySchemaFactories.PrimitiveTypes;
using SchemaInfoScanner.Schemata.TypedPropertySchemata.ContainerTypes;
using SchemaInfoScanner.Schemata.TypedPropertySchemata.RecordTypes;
using SchemaInfoScanner.TypeCheckers;

namespace SchemaInfoScanner.Schemata.TypedPropertySchemaFactories.RecordTypes;

public static class PrimitiveKeyRecordValueDictionaryPropertySchemaFactory
{
    public static PropertySchemaBase Create(
        PropertyName propertyName,
        INamedTypeSymbol propertySymbol,
        IReadOnlyList<AttributeSyntax> attributeList,
        INamedTypeSymbol parentRecordSymbol)
    {
        var keySymbol = (INamedTypeSymbol)propertySymbol.TypeArguments[0];
        if (!PrimitiveTypeChecker.IsSupportedPrimitiveType(keySymbol))
        {
            throw new TypeNotSupportedException($"{propertyName} Key type of dictionary must be a supported primitive type.");
        }

        var valueSymbol = (INamedTypeSymbol)propertySymbol.TypeArguments[1];
        if (!RecordTypeChecker.IsSupportedRecordType(valueSymbol))
        {
            throw new TypeNotSupportedException($"{propertyName} Value type of dictionary must be a supported record type.");
        }

        var keySchema = new PrimitiveTypeGenericArgumentSchema(
            PrimitiveTypeGenericArgumentSchema.ContainerKind.DictionaryKey,
            PrimitivePropertySchemaFactory.Create(propertyName, keySymbol, attributeList));

        var valueSchema = new RecordTypeGenericArgumentSchema(
            RecordTypeGenericArgumentSchema.ContainerKind.DictionaryValue,
            RecordPropertySchemaFactory.Create(propertyName, valueSymbol, attributeList, parentRecordSymbol));

        return new PrimitiveKeyRecordValueDictionarySchema(keySchema, valueSchema, propertySymbol, attributeList);
    }
}
