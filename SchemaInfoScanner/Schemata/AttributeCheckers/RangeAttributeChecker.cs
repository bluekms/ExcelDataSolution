using System.Globalization;
using SchemaInfoScanner.Extensions;
using StaticDataAttribute;

namespace SchemaInfoScanner.Schemata.AttributeCheckers;

public static class RangeAttributeChecker
{
    public static void Check<T>(ParameterSchemaBase parameterSchema, T value)
        where T : IComparable<T>
    {
        if (typeof(T).IsEnum)
        {
            throw new InvalidOperationException("RangeAttribute cannot be used in enum.");
        }

        var attributeValues = parameterSchema.GetAttributeValueList<RangeAttribute>();
        if (!attributeValues.Any())
        {
            return;
        }

        if (attributeValues.Count != 2)
        {
            throw new InvalidOperationException("RangeAttribute must have two values.");
        }

        var min = (T)Convert.ChangeType(attributeValues[0], typeof(T), CultureInfo.InvariantCulture);
        var max = (T)Convert.ChangeType(attributeValues[1], typeof(T), CultureInfo.InvariantCulture);

        if (value.CompareTo(min) < 0 || value.CompareTo(max) > 0)
        {
            throw new ArgumentOutOfRangeException(parameterSchema.ParameterName.FullName, value, $"Value({value}) must be between {min} and {max}.");
        }
    }
}
