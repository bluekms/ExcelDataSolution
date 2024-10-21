using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.Schemata;
using SchemaInfoScanner.TypeCheckers;
using StaticDataAttribute;

namespace SchemaInfoScanner.Extensions;

public static partial class RecordSchemaParameterFlattener
{
    [GeneratedRegex("\\[.*?\\]")]
    private static partial Regex RegexForIndex();

    public static IReadOnlyList<string> Flatten(
        this RawRecordSchema rawRecordSchema,
        RecordSchemaContainer recordSchemaContainer,
        IReadOnlyDictionary<string, int> headerLengths,
        ILogger logger)
    {
        return OnFlatten(rawRecordSchema, recordSchemaContainer, headerLengths, string.Empty, logger);
    }

    private static List<string> OnFlatten(
        this RawRecordSchema rawRecordSchema,
        RecordSchemaContainer recordSchemaContainer,
        IReadOnlyDictionary<string, int> containerLengths,
        string parentPrefix,
        ILogger logger)
    {
        var headers = new List<string>();

        foreach (var parameter in rawRecordSchema.RawParameterSchemaList)
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
                    headers.AddRange(HandlePrimitiveContainer(containerLengths, headerName, logger));
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
                    var indexRange = GetStartIndexAndLength(containerLengths, headerName, logger);
                    for (var i = 0; i < indexRange.Length; ++i)
                    {
                        var index = indexRange.Start + i;

                        var innerFlatten = innerRecordSchema.OnFlatten(
                            recordSchemaContainer,
                            containerLengths,
                            $"{headerName}[{index}]",
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
                    var indexRange = GetStartIndexAndLength(containerLengths, headerName, logger);
                    for (var i = 0; i < indexRange.Length; ++i)
                    {
                        var index = indexRange.Start + i;

                        var innerFlatten = innerRecordSchema.OnFlatten(
                            recordSchemaContainer,
                            containerLengths,
                            $"{headerName}[{index}]",
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
                        headerName,
                        logger);

                    headers.AddRange(innerFlatten);
                }
            }
        }

        return headers;
    }

    private sealed record StartIndexAndLength(int Start, int Length);

    private static StartIndexAndLength GetStartIndexAndLength(
        IReadOnlyDictionary<string, int> containerLengths,
        string headerName,
        ILogger logger)
    {
        var headerNameWithoutIndex = RegexForIndex().Replace(headerName, string.Empty);
        if (!containerLengths.TryGetValue(headerNameWithoutIndex, out var length))
        {
            LogInformation(logger, "Cannot find length for", headerNameWithoutIndex, null);
        }

        return new(0, length);
    }

    private static List<string> HandlePrimitiveContainer(
        IReadOnlyDictionary<string, int> containerLengths,
        string headerName,
        ILogger logger)
    {
        var headers = new List<string>();

        var indexRange = GetStartIndexAndLength(containerLengths, headerName, logger);
        for (var i = 0; i < indexRange.Length; ++i)
        {
            var index = indexRange.Start + i;
            headers.Add($"{headerName}[{index}]");
        }

        return headers;
    }

    private static readonly Action<ILogger, string, string, Exception?> LogInformation =
        LoggerMessage.Define<string, string>(
            LogLevel.Information, new EventId(0, nameof(RecordSchemaParameterFlattener)), "{Message} {Argument}");
}
