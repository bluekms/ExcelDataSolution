using SchemaInfoScanner.Extensions;
using StaticDataAttribute;

namespace SchemaInfoScanner.Schemata.AttributeCheckers;

public static class NullStringAttributeChecker
{
    public sealed record Result(bool IsNull);

    public static Result Check(ParameterSchemaBase parameterSchema, string argument)
    {
        var nullString = string.Empty;
        if (parameterSchema.TryGetAttributeValue<NullStringAttribute, string>(0, out var attributeValue))
        {
            nullString = attributeValue;
        }

        return new(argument == nullString);
    }
}
