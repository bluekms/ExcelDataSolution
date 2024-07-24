using System.Collections.Frozen;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.TypeCheckers;
using StaticDataAttribute;

namespace SchemaInfoScanner.Schemata;

public static partial class RecordSchemaFlattener
{
    private static readonly Action<ILogger, string, Exception?> LogInformation =
        LoggerMessage.Define<string>(LogLevel.Information, new EventId(0, nameof(RecordSchemaFlattener)), "{Message}");

    [GeneratedRegex("\\[.*?\\]")]
    private static partial Regex RegexForIndex();

    public static List<string> Flatten(
        this RecordSchema recordSchema,
        RecordSchemaContainer recordSchemaContainer,
        FrozenDictionary<string, int> containerLengths,
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
        FrozenDictionary<string, int> containerLengths,
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
                headers.AddRange(HandlePrimitiveContainer(containerLengths, indexingMode, headerName, logger));
            }
            else if (DictionaryTypeChecker.IsSupportedDictionaryType(parameter.NamedTypeSymbol))
            {
                var typeArgument = (INamedTypeSymbol)parameter.NamedTypeSymbol.TypeArguments.Last();
                var innerRecordName = new RecordName(typeArgument);
                if (!recordSchemaContainer.RecordSchemaDictionary.TryGetValue(innerRecordName, out var innerRecordSchema))
                {
                    LogInformation(logger, $"Cannot find {innerRecordName}", null);
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
                    LogInformation(logger, $"Cannot find {innerRecordName}", null);
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
        FrozenDictionary<string, int> containerLengths,
        string headerName,
        ILogger logger)
    {
        var headerNameWithoutIndex = RegexForIndex().Replace(headerName, string.Empty);
        if (!containerLengths.TryGetValue(headerNameWithoutIndex, out var length))
        {
            LogInformation(logger, $"Cannot find length for {headerNameWithoutIndex}", null);
        }

        return indexingMode is IndexingMode.ZeroBased
            ? new(0, length)
            : new(1, length + 1);
    }

    public static HashSet<string> FindLengthRequiredNames(
        this RecordSchema recordSchema,
        RecordSchemaContainer recordSchemaContainer,
        string parentPrefix = "")
    {
        var results = new HashSet<string>();
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

            if (ContainerTypeChecker.IsPrimitiveContainer(parameter.NamedTypeSymbol))
            {
                results.Add(headerName);
            }
            else if (DictionaryTypeChecker.IsSupportedDictionaryType(parameter.NamedTypeSymbol))
            {
                results.Add(headerName);
                var typeArgument = (INamedTypeSymbol)parameter.NamedTypeSymbol.TypeArguments.Last();
                var innerRecordName = new RecordName(typeArgument);
                if (recordSchemaContainer.RecordSchemaDictionary.TryGetValue(innerRecordName, out var innerRecordSchema))
                {
                    var innerCollectionNames = innerRecordSchema.FindLengthRequiredNames(recordSchemaContainer, headerName);
                    foreach (var innerName in innerCollectionNames)
                    {
                        results.Add(innerName);
                    }
                }
            }
            else if (ContainerTypeChecker.IsSupportedContainerType(parameter.NamedTypeSymbol))
            {
                results.Add(headerName);
                var typeArgument = (INamedTypeSymbol)parameter.NamedTypeSymbol.TypeArguments.Single();
                var innerRecordName = new RecordName(typeArgument);
                if (recordSchemaContainer.RecordSchemaDictionary.TryGetValue(innerRecordName, out var innerRecordSchema))
                {
                    var innerCollectionNames = innerRecordSchema.FindLengthRequiredNames(recordSchemaContainer, headerName);
                    foreach (var innerName in innerCollectionNames)
                    {
                        results.Add(innerName);
                    }
                }
            }
            else
            {
                var innerRecordName = new RecordName(parameter.NamedTypeSymbol);
                if (recordSchemaContainer.RecordSchemaDictionary.TryGetValue(innerRecordName, out var innerRecordSchema))
                {
                    var innerCollectionNames = innerRecordSchema.FindLengthRequiredNames(recordSchemaContainer, headerName);
                    foreach (var innerName in innerCollectionNames)
                    {
                        results.Add(innerName);
                    }
                }
            }
        }

        return results;
    }

    private static List<string> HandlePrimitiveContainer(
        FrozenDictionary<string, int> containerLengths,
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
}
