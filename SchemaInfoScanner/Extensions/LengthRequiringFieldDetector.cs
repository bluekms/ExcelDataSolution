using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.Schemata;
using SchemaInfoScanner.TypeCheckers;
using StaticDataAttribute;

namespace SchemaInfoScanner.Extensions;

public static class LengthRequiringFieldDetector
{
    public static HashSet<string> Detect(
        RawRecordSchema rawRecordSchema,
        RecordSchemaContainer recordSchemaContainer,
        ILogger logger)
    {
        try
        {
            return OnDetectLengthRequiringFields(
                rawRecordSchema,
                recordSchemaContainer,
                string.Empty);
        }
        catch (Exception exception)
        {
            LogError(logger, exception.Message, exception);
            throw;
        }
    }

    private static HashSet<string> OnDetectLengthRequiringFields(
        RawRecordSchema rawRecordSchema,
        RecordSchemaContainer recordSchemaContainer,
        string parentPrefix)
    {
        var results = new HashSet<string>();

        foreach (var parameter in rawRecordSchema.RawParameterSchemaList)
        {
            if (PrimitiveTypeChecker.IsSupportedPrimitiveType(parameter.NamedTypeSymbol))
            {
                continue;
            }

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
                if (!parameter.HasAttribute<SingleColumnContainerAttribute>())
                {
                    results.Add(headerName);
                }
            }
            else if (DictionaryTypeChecker.IsSupportedDictionaryType(parameter.NamedTypeSymbol))
            {
                results.Add(headerName);

                var typeArgument = (INamedTypeSymbol)parameter.NamedTypeSymbol.TypeArguments.Last();
                var innerRecordSchema = recordSchemaContainer.Find(typeArgument);

                var innerCollectionNames = OnDetectLengthRequiringFields(
                    innerRecordSchema,
                    recordSchemaContainer,
                    headerName);

                foreach (var innerName in innerCollectionNames)
                {
                    results.Add(innerName);
                }
            }
            else if (ContainerTypeChecker.IsSupportedContainerType(parameter.NamedTypeSymbol))
            {
                results.Add(headerName);

                var typeArgument = (INamedTypeSymbol)parameter.NamedTypeSymbol.TypeArguments.Single();
                var innerRecordSchema = recordSchemaContainer.Find(typeArgument);

                var innerCollectionNames = OnDetectLengthRequiringFields(
                    innerRecordSchema,
                    recordSchemaContainer,
                    headerName);

                foreach (var innerName in innerCollectionNames)
                {
                    results.Add(innerName);
                }
            }
            else
            {
                var innerRecordSchema = recordSchemaContainer.Find(parameter.NamedTypeSymbol);

                var innerCollectionNames = OnDetectLengthRequiringFields(
                    innerRecordSchema,
                    recordSchemaContainer,
                    headerName);

                foreach (var innerName in innerCollectionNames)
                {
                    results.Add(innerName);
                }
            }
        }

        return results;
    }

    private static readonly Action<ILogger, string, Exception?> LogError =
        LoggerMessage.Define<string>(LogLevel.Error, new EventId(0, nameof(LogError)), "{Message}");
}
