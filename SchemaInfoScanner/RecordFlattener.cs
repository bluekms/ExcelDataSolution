using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.Extensions;
using SchemaInfoScanner.Schemata;
using SchemaInfoScanner.TypeCheckers;
using StaticDataAttribute;

namespace SchemaInfoScanner;

public static partial class RecordFlattener
{
    public static IReadOnlyList<string> Flatten(
        RecordSchema recordSchema,
        RecordSchemaContainer recordSchemaContainer,
        IReadOnlyDictionary<string, int> headerLengths,
        ILogger logger)
    {
        return OnFlatten(
            recordSchema,
            recordSchemaContainer,
            headerLengths,
            string.Empty,
            logger);
    }

    public static IReadOnlyList<string> OnFlatten(
        RecordSchema recordSchema,
        RecordSchemaContainer recordSchemaContainer,
        IReadOnlyDictionary<string, int> headerLengths,
        string parentPrefix,
        ILogger logger)
    {
        var headers = new List<string>();

        foreach (var parameter in recordSchema.RecordParameterSchemaList)
        {
            var name = parameter.TryGetAttributeValue<ColumnNameAttribute, string>(0, out var columnName)
                ? columnName
                : parameter.PropertyName.Name;

            var headerName = string.IsNullOrEmpty(parentPrefix)
                ? name
                : $"{parentPrefix}.{name}";

            if (PrimitiveTypeChecker.IsSupportedPrimitiveType(parameter.NamedTypeSymbol))
            {
                headers.Add(headerName);
            }
            else if (ContainerTypeChecker.IsPrimitiveContainer(parameter.NamedTypeSymbol))
            {
                if (parameter.HasAttribute<SingleColumnContainerAttribute>())
                {
                    headers.Add(headerName);
                }
                else
                {
                    var result = FlattenPrimitiveContainer(headerName, headerLengths, logger);
                    headers.AddRange(result);
                }
            }
            else if (DictionaryTypeChecker.IsSupportedDictionaryType(parameter.NamedTypeSymbol))
            {
                var typeArgument = (INamedTypeSymbol)parameter.NamedTypeSymbol.TypeArguments.Last();
                var innerRecordSchema = recordSchemaContainer.Find(typeArgument);

                var length = ParseLength(headerLengths, headerName, logger);
                for (var i = 0; i < length; ++i)
                {
                    var innerFlattenResult = OnFlatten(
                        innerRecordSchema,
                        recordSchemaContainer,
                        headerLengths,
                        $"{headerName}[{i}]",
                        logger);

                    headers.AddRange(innerFlattenResult);
                }
            }
            else if (ContainerTypeChecker.IsSupportedContainerType(parameter.NamedTypeSymbol))
            {
                var typeArgument = (INamedTypeSymbol)parameter.NamedTypeSymbol.TypeArguments.Single();
                var innerRecordSchema = recordSchemaContainer.Find(typeArgument);

                var length = ParseLength(headerLengths, headerName, logger);
                for (var i = 0; i < length; ++i)
                {
                    var innerFlattenResult = OnFlatten(
                        innerRecordSchema,
                        recordSchemaContainer,
                        headerLengths,
                        $"{headerName}[{i}]",
                        logger);

                    headers.AddRange(innerFlattenResult);
                }
            }
            else
            {
                var innerRecordSchema = recordSchemaContainer.Find(parameter.NamedTypeSymbol);

                var innerFlatten = OnFlatten(
                    innerRecordSchema,
                    recordSchemaContainer,
                    headerLengths,
                    headerName,
                    logger);

                headers.AddRange(innerFlatten);
            }
        }

        return headers;
    }

    private static List<string> FlattenPrimitiveContainer(
        string headerName,
        IReadOnlyDictionary<string, int> containerLengths,
        ILogger logger)
    {
        var headers = new List<string>();

        var length = ParseLength(containerLengths, headerName, logger);
        for (var i = 0; i < length; ++i)
        {
            headers.Add($"{headerName}[{i}]");
        }

        return headers;
    }

    [GeneratedRegex("\\[.*?\\]")]
    private static partial Regex IndexRegex();

    private static int ParseLength(
        IReadOnlyDictionary<string, int> containerLengths,
        string headerName,
        ILogger logger)
    {
        var headerNameWithoutIndex = IndexRegex().Replace(headerName, string.Empty);
        if (!containerLengths.TryGetValue(headerNameWithoutIndex, out var length))
        {
            LogInformation(logger, "Cannot find length for", headerNameWithoutIndex, null);
        }

        return length;
    }

    private static readonly Action<ILogger, string, string, Exception?> LogInformation =
        LoggerMessage.Define<string, string>(
            LogLevel.Information, new EventId(0, nameof(RecordFlattener)), "{Message} {Argument}");
}
