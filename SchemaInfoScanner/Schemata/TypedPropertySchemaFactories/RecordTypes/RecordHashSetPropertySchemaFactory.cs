using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.Schemata.TypedPropertySchemata.RecordTypes;
using SchemaInfoScanner.TypeCheckers;

namespace SchemaInfoScanner.Schemata.TypedPropertySchemaFactories.RecordTypes;

public class RecordHashSetPropertySchemaFactory
{
    public static PropertySchemaBase Create(
        PropertyName propertyName,
        INamedTypeSymbol propertySymbol,
        IReadOnlyList<AttributeSyntax> attributeList,
        INamedTypeSymbol parentRecordSymbol)
    {
        if (!HashSetTypeChecker.IsSupportedHashSetType(propertySymbol))
        {
            throw new InvalidOperationException($"{propertyName} is not a supported hash set type.");
        }

        if (HashSetTypeChecker.IsPrimitiveHashSetType(propertySymbol))
        {
            throw new InvalidOperationException($"{propertyName} is record hash set type.");
        }

        var typeArgumentSymbol = (INamedTypeSymbol)propertySymbol.TypeArguments.Single();
        var nestedSchema = RecordPropertySchemaFactory.Create(
            propertyName,
            typeArgumentSymbol,
            attributeList,
            parentRecordSymbol);

        var genericArgumentSchema = new RecordTypeGenericArgumentSchema(
            RecordTypeGenericArgumentSchema.CatalogKind.HashSet,
            nestedSchema);

        return new RecordHashSetPropertySchema(
            genericArgumentSchema,
            propertySymbol,
            attributeList);
    }
}
