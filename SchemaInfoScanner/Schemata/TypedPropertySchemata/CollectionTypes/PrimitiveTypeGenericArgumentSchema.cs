using Microsoft.Extensions.Logging;
using SchemaInfoScanner.NameObjects;

namespace SchemaInfoScanner.Schemata.TypedPropertySchemata.CollectionTypes;

public sealed record PrimitiveTypeGenericArgumentSchema(
    PrimitiveTypeGenericArgumentSchema.CollectionKind ContainingType,
    PropertySchemaBase NestedSchema)
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

    public PropertyName PropertyName { get; } = NestedSchema.PropertyName;
    public string Name { get; } = $"{NestedSchema.PropertyName.Name}'s <{NestedSchema.GetType().Name}>";
    public string FullName { get; } = $"{NestedSchema.PropertyName.FullName}'s <{NestedSchema.GetType().Name}>";

    public int CheckCompatibility(CompatibilityContext context, ILogger logger)
    {
        return NestedSchema.CheckCompatibility(context, logger);
    }
}
