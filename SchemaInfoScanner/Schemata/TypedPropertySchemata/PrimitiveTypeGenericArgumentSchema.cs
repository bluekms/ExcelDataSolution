using SchemaInfoScanner.NameObjects;

namespace SchemaInfoScanner.Schemata.TypedPropertySchemata;

public class PrimitiveTypeGenericArgumentSchema(
    PrimitiveTypeGenericArgumentSchema.ContainerKind containingType,
    PropertySchemaBase innerSchema)
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
    public PropertyName PropertyName { get; } = innerSchema.PropertyName;
    public PropertySchemaBase InnerSchema { get; } = innerSchema;
    public string Name { get; } = $"{innerSchema.PropertyName.Name}'s <{innerSchema.GetType().Name}>";
    public string FullName { get; } = $"{innerSchema.PropertyName.FullName}'s <{innerSchema.GetType().Name}>";
}
