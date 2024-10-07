using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.Schemata.RecordParameterSchemaExtensions;
using SchemaInfoScanner.TypeCheckers;
using StaticDataAttribute;

namespace SchemaInfoScanner.Schemata.RecordSchemaExtensions;

public static partial class ParameterFlattener
{
    [GeneratedRegex("\\[.*?\\]")]
    private static partial Regex RegexForIndex();

    public static IReadOnlyList<string> Flatten(
        this RecordSchema recordSchema,
        RecordSchemaContainer recordSchemaContainer,
        IReadOnlyDictionary<string, int> containerLengths,
        ILogger logger)
    {
        var globalIndexingMode = (IndexingMode?)null;
        if (recordSchema.TryGetAttributeValue<RecordGlobalIndexingModeAttribute, IndexingMode>(0, out var mode))
        {
            globalIndexingMode = mode;
        }

        return OnFlatten(recordSchema, recordSchemaContainer, containerLengths, globalIndexingMode, string.Empty, logger);
    }

    private static List<string> OnFlatten(
        this RecordSchema recordSchema,
        RecordSchemaContainer recordSchemaContainer,
        IReadOnlyDictionary<string, int> containerLengths,
        IndexingMode? globalIndexingMode,
        string parentPrefix,
        ILogger logger)
    {
        var headers = new List<string>();
        var indexingMode = DetermineIndexingMode(globalIndexingMode, recordSchema);

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
                if (parameter.HasAttribute<SingleColumnContainerAttribute>())
                {
                    headers.Add(headerName);
                }
                else
                {
                    headers.AddRange(HandlePrimitiveContainer(containerLengths, indexingMode, headerName, logger));
                }
            }
            else if (DictionaryTypeChecker.IsSupportedDictionaryType(parameter.NamedTypeSymbol))
            {
                var typeArgument = (INamedTypeSymbol)parameter.NamedTypeSymbol.TypeArguments.Last();
                var innerRecordName = new RecordName(typeArgument);
                if (!recordSchemaContainer.RecordSchemaDictionary.TryGetValue(innerRecordName, out var innerRecordSchema))
                {
                    LogInformation(logger, "Cannot find", innerRecordName.FullName, null);
                }
                else
                {
                    var indexRange = GetIndexRange(indexingMode, containerLengths, headerName, logger);
                    for (var i = indexRange.Start; i < indexRange.ExclusiveEnd; ++i)
                    {
                        var innerFlatten = innerRecordSchema.OnFlatten(
                            recordSchemaContainer,
                            containerLengths,
                            globalIndexingMode,
                            $"{headerName}[{i}]",
                            logger);

                        headers.AddRange(innerFlatten);
                    }
                }
            }
            else if (ContainerTypeChecker.IsSupportedContainerType(parameter.NamedTypeSymbol))
            {
                var typeArgument = (INamedTypeSymbol)parameter.NamedTypeSymbol.TypeArguments.Single();
                var innerRecordName = new RecordName(typeArgument);
                if (!recordSchemaContainer.RecordSchemaDictionary.TryGetValue(innerRecordName, out var innerRecordSchema))
                {
                    LogInformation(logger, "Cannot find", innerRecordName.FullName, null);
                }
                else
                {
                    var indexRange = GetIndexRange(indexingMode, containerLengths, headerName, logger);
                    for (var i = indexRange.Start; i < indexRange.ExclusiveEnd; ++i)
                    {
                        var innerFlatten = innerRecordSchema.OnFlatten(
                            recordSchemaContainer,
                            containerLengths,
                            globalIndexingMode,
                            $"{headerName}[{i}]",
                            logger);

                        headers.AddRange(innerFlatten);
                    }
                }
            }
            else
            {
                var innerRecordName = new RecordName(parameter.NamedTypeSymbol);
                if (recordSchemaContainer.RecordSchemaDictionary.TryGetValue(innerRecordName, out var innerRecordSchema))
                {
                    var innerFlatten = innerRecordSchema.OnFlatten(
                        recordSchemaContainer,
                        containerLengths,
                        globalIndexingMode,
                        headerName,
                        logger);

                    headers.AddRange(innerFlatten);
                }
            }
        }

        return headers;
    }

    private static IndexingMode DetermineIndexingMode(IndexingMode? globalIndexingMode, RecordSchema recordSchema)
    {
        if (globalIndexingMode is not null)
        {
            return globalIndexingMode.Value;
        }

        if (recordSchema.TryGetAttributeValue<IndexingModeAttribute, IndexingMode>(0, out var mode))
        {
            return mode;
        }

        return IndexingMode.ZeroBased;
    }

    private sealed record IndexRange(int Start, int ExclusiveEnd);

    private static IndexRange GetIndexRange(
        IndexingMode indexingMode,
        IReadOnlyDictionary<string, int> containerLengths,
        string headerName,
        ILogger logger)
    {
        var headerNameWithoutIndex = RegexForIndex().Replace(headerName, string.Empty);
        if (!containerLengths.TryGetValue(headerNameWithoutIndex, out var length))
        {
            LogInformation(logger, "Cannot find length for", headerNameWithoutIndex, null);
        }

        return indexingMode switch
        {
            IndexingMode.OneBased => new(1, length),
            IndexingMode.ZeroBased => new(0, length),
            _ => throw new ArgumentOutOfRangeException(nameof(indexingMode), indexingMode, null)
        };
    }

    private static List<string> HandlePrimitiveContainer(
        IReadOnlyDictionary<string, int> containerLengths,
        IndexingMode indexingMode,
        string headerName,
        ILogger logger)
    {
        var headers = new List<string>();

        var indexRange = GetIndexRange(indexingMode, containerLengths, headerName, logger);
        for (var i = indexRange.Start; i < indexRange.ExclusiveEnd; ++i)
        {
            headers.Add($"{headerName}[{i}]");
        }

        return headers;
    }

    private static readonly Action<ILogger, string, string, Exception?> LogInformation =
        LoggerMessage.Define<string, string>(
            LogLevel.Information, new EventId(0, nameof(ParameterFlattener)), "{Message} {Argument}");
}
