using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.Schemata.TypedPropertySchemaFactories.PrimitiveTypes;
using SchemaInfoScanner.Schemata.TypedPropertySchemata.ContainerTypes;
using SchemaInfoScanner.Schemata.TypedPropertySchemata.RecordTypes;
using SchemaInfoScanner.TypeCheckers;

namespace SchemaInfoScanner.Schemata.TypedPropertySchemaFactories.RecordTypes;

public static class RecordKeyAndValueDictionaryPropertySchemaFactory
{
    public static PropertySchemaBase Create(
        PropertyName propertyName,
        INamedTypeSymbol propertySymbol,
        IReadOnlyList<AttributeSyntax> attributeList,
        INamedTypeSymbol parentRecordSymbol)
    {
        var keySymbol = (INamedTypeSymbol)propertySymbol.TypeArguments[0];
        if (!RecordTypeChecker.IsSupportedRecordType(keySymbol))
        {
            throw new InvalidOperationException($"{propertyName} Key type of dictionary must be a supported record type.");
        }

        var valueSymbol = (INamedTypeSymbol)propertySymbol.TypeArguments[1];
        if (!RecordTypeChecker.IsSupportedRecordType(valueSymbol))
        {
            throw new NotSupportedException($"{propertyName} Value type of dictionary must be a supported record type.");
        }

        var keySchema = new RecordTypeGenericArgumentSchema(
            RecordTypeGenericArgumentSchema.ContainerKind.DictionaryKey,
            RecordPropertySchemaFactory.Create(propertyName, keySymbol, attributeList, parentRecordSymbol));

        var valueSchema = new RecordTypeGenericArgumentSchema(
            RecordTypeGenericArgumentSchema.ContainerKind.DictionaryValue,
            RecordPropertySchemaFactory.Create(propertyName, valueSymbol, attributeList, parentRecordSymbol));

        return new RecordKeyRecordValueDictionarySchema(keySchema, valueSchema, propertySymbol, attributeList);
    }
}
