using SchemaInfoScanner.NameObjects;

namespace SchemaInfoScanner.Schemata.TypedPropertySchemata.RecordTypes;

public class RecordTypeGenericArgumentSchema(
    RecordTypeGenericArgumentSchema.CatalogKind catalogType,
    PropertySchemaBase nestedSchema)
{
    public enum CatalogKind
    {
        List,
        HashSet,
        DictionaryKey,
        DictionaryValue,
    }

    public CatalogKind CatalogType { get; } = catalogType;
    public PropertyName PropertyName { get; } = nestedSchema.PropertyName;
    public PropertySchemaBase NestedSchema { get; } = nestedSchema;
    public string Name { get; } = $"{nestedSchema.PropertyName.Name}'s <{nestedSchema.GetType().Name}>";
    public string FullName { get; } = $"{nestedSchema.PropertyName.FullName}'s <{nestedSchema.GetType().Name}>";
}
