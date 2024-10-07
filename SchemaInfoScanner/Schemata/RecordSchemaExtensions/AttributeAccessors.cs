using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SchemaInfoScanner.Schemata.RecordSchemaExtensions;

public static class AttributeAccessors
{
    public static bool HasAttribute<T>(
        this RecordSchema recordSchema)
        where T : Attribute
    {
        var attributeName = typeof(T).Name.Replace("Attribute", string.Empty);
        return recordSchema.RecordAttributeList.Any(x => x.Name.ToString() == attributeName);
    }

    public static TValue GetAttributeValue<TAttribute, TValue>(
        this RecordSchema recordSchema,
        int attributeParameterIndex)
        where TAttribute : Attribute
    {
        var attributeName = typeof(TAttribute).Name.Replace("Attribute", string.Empty);
        var attribute = recordSchema.RecordAttributeList.Single(x => x.Name.ToString() == attributeName);

        if (attribute.ArgumentList is null)
        {
            throw new ArgumentNullException($"{typeof(TAttribute).Name} has no property.");
        }

        var argument = attribute.ArgumentList.Arguments[attributeParameterIndex].Expression;
        var valueString = argument switch
        {
            LiteralExpressionSyntax literal => literal.Token.ValueText,
            MemberAccessExpressionSyntax memberAccess => memberAccess.Name.Identifier.Text,
            _ => throw new InvalidOperationException("Unsupported expression type."),
        };

        return typeof(TValue).IsEnum
            ? (TValue)Enum.Parse(typeof(TValue), valueString.Split('.')[^1])
            : (TValue)Convert.ChangeType(valueString, typeof(TValue), CultureInfo.InvariantCulture);
    }

    public static bool TryGetAttributeValue<TAttribute, TValue>(
        this RecordSchema recordSchema,
        int attributeParameterIndex,
        [NotNullWhen(true)] out TValue? value)
        where TAttribute : Attribute
    {
        try
        {
            value = recordSchema.GetAttributeValue<TAttribute, TValue>(attributeParameterIndex);
            return value is not null;
        }
        catch (Exception)
        {
            value = default;
            return false;
        }
    }

    public static IReadOnlyList<string> GetAttributeValueList<TAttribute>(this RecordSchema recordSchema)
    {
        var attributeName = typeof(TAttribute).Name.Replace("Attribute", string.Empty);
        var attribute = recordSchema.RecordAttributeList.Single(x => x.Name.ToString() == attributeName);

        if (attribute.ArgumentList is null)
        {
            throw new ArgumentNullException($"{typeof(TAttribute).Name} has no property.");
        }

        return attribute.ArgumentList.Arguments
            .Select(x => x.Expression switch
            {
                LiteralExpressionSyntax literal => literal.Token.ValueText,
                MemberAccessExpressionSyntax memberAccess => memberAccess.Name.Identifier.Text,
                _ => throw new InvalidOperationException("Unsupported expression type."),
            }).ToList();
    }
}
