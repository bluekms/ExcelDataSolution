using SchemaInfoScanner.NameObjects;

namespace SchemaInfoScanner.Schemata.TypedPropertySchemata.CollectionTypes;

public class PrimitiveTypeGenericArgumentSchema(
    PrimitiveTypeGenericArgumentSchema.CollectionKind containingType,
    PropertySchemaBase nestedSchema)
{
    public enum CollectionKind
    {
        SingleColumnList,
        List,
        SingleColumnHashSet,
        HashSet,
        DictionaryKey,
        DictionaryValue,
    }

    public CollectionKind ContainingType { get; } = containingType;
    public PropertyName PropertyName { get; } = nestedSchema.PropertyName;
    public PropertySchemaBase NestedSchema { get; } = nestedSchema;
    public string Name { get; } = $"{nestedSchema.PropertyName.Name}'s <{nestedSchema.GetType().Name}>";
    public string FullName { get; } = $"{nestedSchema.PropertyName.FullName}'s <{nestedSchema.GetType().Name}>";
}
