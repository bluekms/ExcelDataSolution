using SchemaInfoScanner.Extensions;
using StaticDataAttribute;

namespace SchemaInfoScanner.Schemata.AttributeCheckers;

public static class NullStringAttributeChecker
{
    public sealed record Result(bool IsNull);

    public static Result Check(PropertySchemaBase propertySchema, string argument)
    {
        var nullString = string.Empty;
        if (propertySchema.TryGetAttributeValue<NullStringAttribute, string>(0, out var attributeValue))
        {
            nullString = attributeValue;
        }

        return new(argument == nullString);
    }
}
