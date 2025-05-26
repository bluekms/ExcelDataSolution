using SchemaInfoScanner.NameObjects;

namespace SchemaInfoScanner.Schemata.TypedPropertySchemata.ContainerTypes;

public class PrimitiveTypeGenericArgumentSchema(
    PrimitiveTypeGenericArgumentSchema.ContainerKind containingType,
    PropertySchemaBase nestedSchema)
{
    public enum ContainerKind
    {
        SingleColumnList,
        List,
        SingleColumnHashSet,
        HashSet,
        DictionaryKey,
        DictionaryValue,
    }

    public ContainerKind ContainingType { get; } = containingType;
    public PropertyName PropertyName { get; } = nestedSchema.PropertyName;
    public PropertySchemaBase NestedSchema { get; } = nestedSchema;
    public string Name { get; } = $"{nestedSchema.PropertyName.Name}'s <{nestedSchema.GetType().Name}>";
    public string FullName { get; } = $"{nestedSchema.PropertyName.FullName}'s <{nestedSchema.GetType().Name}>";
}
