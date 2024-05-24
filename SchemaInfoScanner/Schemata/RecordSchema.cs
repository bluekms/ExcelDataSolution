using System.Globalization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.NameObjects;
using StaticDataAttribute;

namespace SchemaInfoScanner.Schemata;

public sealed record RecordSchema(
    RecordName RecordName,
    INamedTypeSymbol NamedTypeSymbol,
    IReadOnlyList<AttributeSyntax> RecordAttributeList,
    IReadOnlyList<RecordParameterSchema> RecordParameterSchemaList)
{
    public bool HasAttribute<T>()
    {
        var attributeName = typeof(T).Name.Replace("Attribute", string.Empty);
        return RecordAttributeList.Any(x => x.Name.ToString() == attributeName);
    }

    public TValue GetAttributeValue<TAttribute, TValue>(int attributeParameterIndex = 0)
        where TAttribute : Attribute
    {
        var attributeName = typeof(TAttribute).Name.Replace("Attribute", string.Empty);
        var attribute = RecordAttributeList.Single(x => x.Name.ToString() == attributeName);

        if (attribute.ArgumentList is null)
        {
            throw new ArgumentNullException($"{typeof(TAttribute).Name} has no property.");
        }

        var valueString = attributeParameterIndex is 0
            ? attribute.ArgumentList.Arguments.First().Expression.ToString().Trim('"')
            : attribute.ArgumentList.Arguments[attributeParameterIndex].ToString().Trim('"');

        return typeof(TValue).IsEnum
            ? (TValue)Enum.Parse(typeof(TValue), valueString)
            : (TValue)Convert.ChangeType(valueString, typeof(TValue), CultureInfo.InvariantCulture);
    }

    public string GetSheetName()
    {
        if (!HasAttribute<SheetNameAttribute>())
        {
            return RecordName.Name;
        }

        return GetAttributeValue<SheetNameAttribute, string>();
    }
}
