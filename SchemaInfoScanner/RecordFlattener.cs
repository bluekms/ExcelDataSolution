using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner.Catalogs;
using SchemaInfoScanner.Extensions;
using SchemaInfoScanner.Schemata;
using SchemaInfoScanner.TypeCheckers;
using StaticDataAttribute;

namespace SchemaInfoScanner;

public static class RecordFlattener
{
    public static IReadOnlyList<string> Flatten(
        RecordSchema recordSchema,
        RecordSchemaCatalog recordSchemaCatalog,
        ILogger logger)
    {
        return OnFlatten(
            recordSchema,
            recordSchemaCatalog,
            string.Empty,
            logger);
    }

    private static List<string> OnFlatten(
        RecordSchema recordSchema,
        RecordSchemaCatalog recordSchemaCatalog,
        string parentPrefix,
        ILogger logger)
    {
        var headers = new List<string>();

        foreach (var parameter in recordSchema.PropertySchemata)
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
            else if (CollectionTypeChecker.IsPrimitiveCollection(parameter.NamedTypeSymbol))
            {
                if (parameter.HasAttribute<SingleColumnCollectionAttribute>())
                {
                    headers.Add(headerName);
                }
                else
                {
                    if (!parameter.TryGetAttributeValue<LengthAttribute, int>(out var length))
                    {
                        throw new InvalidOperationException($"Parameter {parameter.PropertyName} cannot have LengthAttribute");
                    }

                    var result = FlattenPrimitiveCollection(headerName, length);
                    headers.AddRange(result);
                }
            }
            else if (MapTypeChecker.IsSupportedMapType(parameter.NamedTypeSymbol))
            {
                var typeArgument = (INamedTypeSymbol)parameter.NamedTypeSymbol.TypeArguments.Last();
                var innerRecordSchema = recordSchemaCatalog.Find(typeArgument);

                if (!parameter.TryGetAttributeValue<LengthAttribute, int>(out var length))
                {
                    throw new InvalidOperationException($"Parameter {parameter.PropertyName} cannot have LengthAttribute");
                }

                for (var i = 0; i < length; ++i)
                {
                    var innerFlattenResult = OnFlatten(
                        innerRecordSchema,
                        recordSchemaCatalog,
                        $"{headerName}[{i}]",
                        logger);

                    headers.AddRange(innerFlattenResult);
                }
            }
            else if (CollectionTypeChecker.IsSupportedCollectionType(parameter.NamedTypeSymbol))
            {
                var typeArgument = (INamedTypeSymbol)parameter.NamedTypeSymbol.TypeArguments.Single();
                var innerRecordSchema = recordSchemaCatalog.Find(typeArgument);

                if (!parameter.TryGetAttributeValue<LengthAttribute, int>(out var length))
                {
                    throw new InvalidOperationException($"Parameter {parameter.PropertyName} cannot have LengthAttribute");
                }

                for (var i = 0; i < length; ++i)
                {
                    var innerFlattenResult = OnFlatten(
                        innerRecordSchema,
                        recordSchemaCatalog,
                        $"{headerName}[{i}]",
                        logger);

                    headers.AddRange(innerFlattenResult);
                }
            }
            else
            {
                var innerRecordSchema = recordSchemaCatalog.Find(parameter.NamedTypeSymbol);

                var innerFlatten = OnFlatten(
                    innerRecordSchema,
                    recordSchemaCatalog,
                    headerName,
                    logger);

                headers.AddRange(innerFlatten);
            }
        }

        return headers;
    }

    private static List<string> FlattenPrimitiveCollection(
        string headerName,
        int length)
    {
        var headers = new List<string>();

        for (var i = 0; i < length; ++i)
        {
            headers.Add($"{headerName}[{i}]");
        }

        return headers;
    }

    private static readonly Regex IndexRegex = new(@"\[.*?\]");

    private static int ParseLength(
        IReadOnlyDictionary<string, int> collectionLengths,
        string headerName,
        ILogger logger)
    {
        var headerNameWithoutIndex = IndexRegex.Replace(headerName, string.Empty);
        if (!collectionLengths.TryGetValue(headerNameWithoutIndex, out var length))
        {
            LogInformation(logger, "Cannot find length for", headerNameWithoutIndex, null);
        }

        return length;
    }

    private static readonly Action<ILogger, string, string, Exception?> LogInformation =
        LoggerMessage.Define<string, string>(
            LogLevel.Information, new EventId(0, nameof(RecordFlattener)), "{Message} {Argument}");
}
