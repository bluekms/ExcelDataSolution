using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.Schemata;

namespace SchemaInfoScanner.Extensions;

public static class ParameterSchemaBaseAttributeAccessors
{
    public static bool HasAttribute<T>(
        this ParameterSchemaBase parameterSchema)
        where T : Attribute
    {
        var attributeName = typeof(T).Name.Replace("Attribute", string.Empty);
        return parameterSchema.AttributeList.Any(x => x.Name.ToString() == attributeName);
    }

    public static TValue GetAttributeValue<TAttribute, TValue>(
        this ParameterSchemaBase parameterSchema,
        int attributeParameterIndex = 0)
        where TAttribute : Attribute
    {
        var attributeName = typeof(TAttribute).Name.Replace("Attribute", string.Empty);
        var attribute = parameterSchema.AttributeList.Single(x => x.Name.ToString() == attributeName);

        if (attribute.ArgumentList is null)
        {
            throw new ArgumentNullException($"{typeof(TAttribute).Name} has no property.");
        }

        var valueString = attribute.ArgumentList.Arguments[attributeParameterIndex].ToString().Trim('"');
        return typeof(TValue).IsEnum
            ? (TValue)Enum.Parse(typeof(TValue), valueString.Split('.')[^1])
            : (TValue)Convert.ChangeType(valueString, typeof(TValue), CultureInfo.InvariantCulture);
    }

    public static bool TryGetAttributeValue<TAttribute, TValue>(
        this ParameterSchemaBase parameterSchema,
        int attributeParameterIndex,
        [NotNullWhen(true)] out TValue? value)
        where TAttribute : Attribute
    {
        try
        {
            value = parameterSchema.GetAttributeValue<TAttribute, TValue>(attributeParameterIndex);
            return value is not null;
        }
        catch (Exception)
        {
            value = default;
            return false;
        }
    }

    public static IReadOnlyList<string> GetAttributeValueList<TAttribute>(this ParameterSchemaBase parameterSchema)
    {
        var attributeName = typeof(TAttribute).Name.Replace("Attribute", string.Empty);
        var attribute = parameterSchema.AttributeList.Single(x => x.Name.ToString() == attributeName);

        if (attribute.ArgumentList is null)
        {
            throw new ArgumentNullException($"{typeof(TAttribute).Name} has no property.");
        }

        return attribute.ArgumentList.Arguments
            .Select(x => x.Expression switch
            {
                LiteralExpressionSyntax literal => literal.Token.ValueText,
                MemberAccessExpressionSyntax memberAccess => memberAccess.Name.Identifier.Text, // for enum
                _ => throw new InvalidOperationException("Unsupported expression type."),
            }).ToList();
    }
}
