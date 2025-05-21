using SchemaInfoScanner.NameObjects;

namespace SchemaInfoScanner.Schemata.TypedParameterSchemata;

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
    public ParameterName ParameterName { get; } = innerSchema.ParameterName;
    public PropertySchemaBase InnerSchema { get; } = innerSchema;
    public string Name { get; } = $"{innerSchema.ParameterName.Name}'s <{innerSchema.GetType().Name}>";
    public string FullName { get; } = $"{innerSchema.ParameterName.FullName}'s <{innerSchema.GetType().Name}>";
}
