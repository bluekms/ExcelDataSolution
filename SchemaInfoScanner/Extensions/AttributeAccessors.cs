using System.Globalization;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SchemaInfoScanner.Extensions;

public static class AttributeAccessors
{
    public static bool HasAttribute<T>(
        IReadOnlyList<AttributeSyntax> attributeSyntaxList)
        where T : Attribute
    {
        var attributeName = typeof(T).Name.Replace("Attribute", string.Empty);
        return attributeSyntaxList.Any(x => x.Name.ToString() == attributeName);
    }

    public static TValue GetAttributeValue<TAttribute, TValue>(
        IReadOnlyList<AttributeSyntax> attributeSyntaxList,
        int attributeParameterIndex = 0)
        where TAttribute : Attribute
    {
        var attributeName = typeof(TAttribute).Name.Replace("Attribute", string.Empty);
        var attribute = attributeSyntaxList.Single(x => x.Name.ToString() == attributeName);

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
        IReadOnlyList<AttributeSyntax> attributeSyntaxList,
        out TValue value,
        int attributeParameterIndex = 0)
        where TAttribute : Attribute
    {
        if (!HasAttribute<TAttribute>(attributeSyntaxList))
        {
            value = default!;
            return false;
        }

        value = GetAttributeValue<TAttribute, TValue>(attributeSyntaxList, attributeParameterIndex);
        return true;
    }
}
