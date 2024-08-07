﻿using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.NameObjects;

namespace SchemaInfoScanner.Schemata;

public sealed record RecordSchema(
    RecordName RecordName,
    INamedTypeSymbol NamedTypeSymbol,
    ImmutableList<AttributeSyntax> RecordAttributeList,
    ImmutableList<RecordParameterSchema> RecordParameterSchemaList)
{
    public bool HasAttribute<T>()
        where T : Attribute
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

        var valueString = attribute.ArgumentList.Arguments[attributeParameterIndex].ToString().Trim('"');

        return typeof(TValue).IsEnum
            ? (TValue)Enum.Parse(typeof(TValue), valueString.Split('.')[^1])
            : (TValue)Convert.ChangeType(valueString, typeof(TValue), CultureInfo.InvariantCulture);
    }

    public bool TryGetAttributeValue<TAttribute, TValue>(int attributeParameterIndex, [NotNullWhen(true)] out TValue? value)
        where TAttribute : Attribute
    {
        try
        {
            value = GetAttributeValue<TAttribute, TValue>(attributeParameterIndex);
            return value is not null;
        }
        catch (Exception)
        {
            value = default;
            return false;
        }
    }

    public override string ToString()
    {
        return RecordName.FullName;
    }
}
