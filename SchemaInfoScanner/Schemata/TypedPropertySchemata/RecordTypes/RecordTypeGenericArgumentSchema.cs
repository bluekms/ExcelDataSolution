using Microsoft.Extensions.Logging;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.Schemata.CompatibilityContexts;

namespace SchemaInfoScanner.Schemata.TypedPropertySchemata.RecordTypes;

public sealed record RecordTypeGenericArgumentSchema(
    RecordTypeGenericArgumentSchema.CollectionKind ContainingType,
    PropertySchemaBase NestedSchema)
{
    public enum CollectionKind
    {
        List,
        HashSet,
        DictionaryKey,
        DictionaryValue,
    }

    public PropertyName PropertyName { get; } = NestedSchema.PropertyName;
    public string Name { get; } = $"{NestedSchema.PropertyName.Name}'s <{NestedSchema.GetType().Name}>";
    public string FullName { get; } = $"{NestedSchema.PropertyName.FullName}'s <{NestedSchema.GetType().Name}>";

    public int CheckCompatibility(ICompatibilityContext context, ILogger logger)
    {
        return NestedSchema.CheckCompatibility(context, logger);
    }
}
