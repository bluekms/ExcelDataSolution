using System.Collections.Frozen;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.TypeCheckers;
using StaticDataAttribute;

namespace SchemaInfoScanner.Schemata;

public static partial class RecordSchemaFlattener
{
    [GeneratedRegex("\\[.*?\\]")]
    private static partial Regex RegexForIndex();

    public static List<string> Flatten(
        this RecordSchema recordSchema,
        RecordSchemaContainer recordSchemaContainer,
        FrozenDictionary<string, int> containerLengths,
        string parentPrefix = "")
    {
        var headers = new List<string>();

        foreach (var parameter in recordSchema.RecordParameterSchemaList)
        {
            var name = parameter.ParameterName.Name;
            if (parameter.TryGetAttributeValue<ColumnNameAttribute, string>(0, out var columnName))
            {
                name = columnName;
            }

            var headerName = string.IsNullOrEmpty(parentPrefix)
                ? name
                : $"{parentPrefix}.{name}";

            if (PrimitiveTypeChecker.IsSupportedPrimitiveType(parameter.NamedTypeSymbol))
            {
                headers.Add(headerName);
            }
            else if (ContainerTypeChecker.IsPrimitiveContainer(parameter.NamedTypeSymbol))
            {
                headers.AddRange(HandlePrimitiveContainer(headerName, containerLengths));
            }
            else if (DictionaryTypeChecker.IsSupportedDictionaryType(parameter.NamedTypeSymbol))
            {
                var typeArgument = (INamedTypeSymbol)parameter.NamedTypeSymbol.TypeArguments.Last();
                var innerRecordName = new RecordName(typeArgument);
                if (recordSchemaContainer.RecordSchemaDictionary.TryGetValue(innerRecordName, out var innerRecordSchema))
                {
                    var parentPrefixWithoutIndex = RegexForIndex().Replace(headerName, string.Empty);
                    var length = containerLengths.GetValueOrDefault(parentPrefixWithoutIndex, 1);
                    for (var i = 0; i < length; ++i)
                    {
                        headers.AddRange(innerRecordSchema.Flatten(recordSchemaContainer, containerLengths, $"{headerName}[{i}]"));
                    }
                }
            }
            else if (ContainerTypeChecker.IsSupportedContainerType(parameter.NamedTypeSymbol))
            {
                var typeArgument = (INamedTypeSymbol)parameter.NamedTypeSymbol.TypeArguments.Single();
                var innerRecordName = new RecordName(typeArgument);
                if (recordSchemaContainer.RecordSchemaDictionary.TryGetValue(innerRecordName, out var innerRecordSchema))
                {
                    var parentPrefixWithoutIndex = RegexForIndex().Replace(headerName, string.Empty);
                    var length = containerLengths.GetValueOrDefault(parentPrefixWithoutIndex, 1);
                    for (var i = 0; i < length; ++i)
                    {
                        headers.AddRange(innerRecordSchema.Flatten(recordSchemaContainer, containerLengths, $"{headerName}[{i}]"));
                    }
                }
            }
            else
            {
                var innerRecordName = new RecordName(parameter.NamedTypeSymbol);
                if (recordSchemaContainer.RecordSchemaDictionary.TryGetValue(innerRecordName, out var innerRecordSchema))
                {
                    headers.AddRange(innerRecordSchema.Flatten(recordSchemaContainer, containerLengths, headerName));
                }
            }
        }

        return headers;
    }

    private static List<string> HandlePrimitiveContainer(
        string parentPrefix,
        FrozenDictionary<string, int> containerLengths)
    {
        var headers = new List<string>();

        var parentPrefixWithoutIndex = RegexForIndex().Replace(parentPrefix, string.Empty);
        var length = containerLengths.GetValueOrDefault(parentPrefixWithoutIndex, 1);
        for (var i = 0; i < length; ++i)
        {
            headers.Add($"{parentPrefix}[{i}]");
        }

        return headers;
    }
}
