using Microsoft.Extensions.Logging;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.Exceptions;
using SchemaInfoScanner.Extensions;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.Schemata;
using StaticDataAttribute;

namespace SchemaInfoScanner.TypeCheckers;

internal static class SupportedTypeChecker
{
    public static void Check(
        RawParameterSchema rawParameter,
        RecordSchemaContainer recordSchemaContainer,
        HashSet<RecordName> visited,
        ILogger logger)
    {
        if (rawParameter.HasAttribute<IgnoreAttribute>())
        {
            LogTrace(logger, $"{rawParameter.ParameterName.FullName} is ignored.", null);
            return;
        }

        LogTrace(logger, rawParameter.ParameterName.FullName, null);

        if (PrimitiveTypeChecker.IsSupportedPrimitiveType(rawParameter.NamedTypeSymbol))
        {
            PrimitiveTypeChecker.Check(rawParameter);
            return;
        }

        if (ContainerTypeChecker.IsSupportedContainerType(rawParameter.NamedTypeSymbol))
        {
            CheckSupportedContainerType(rawParameter, recordSchemaContainer, visited, logger);
            return;
        }

        var recordName = new RecordName(rawParameter.NamedTypeSymbol);
        if (!recordSchemaContainer.RecordSchemaDictionary.TryGetValue(recordName, out var recordSchema))
        {
            var innerException = new KeyNotFoundException($"{recordName.FullName} is not found in the record schema dictionary.");
            throw new TypeNotSupportedException($"{rawParameter.ParameterName.FullName} is not supported record type.", innerException);
        }

        RecordTypeChecker.Check(recordSchema, recordSchemaContainer, visited, logger);
    }

    private static void CheckSupportedContainerType(
        RawParameterSchema rawParameter,
        RecordSchemaContainer recordSchemaContainer,
        HashSet<RecordName> visited,
        ILogger logger)
    {
        if (HashSetTypeChecker.IsSupportedHashSetType(rawParameter.NamedTypeSymbol))
        {
            HashSetTypeChecker.Check(rawParameter, recordSchemaContainer, visited, logger);
        }
        else if (ListTypeChecker.IsSupportedListType(rawParameter.NamedTypeSymbol))
        {
            ListTypeChecker.Check(rawParameter, recordSchemaContainer, visited, logger);
        }
        else if (DictionaryTypeChecker.IsSupportedDictionaryType(rawParameter.NamedTypeSymbol))
        {
            DictionaryTypeChecker.Check(rawParameter, recordSchemaContainer, visited, logger);
        }
        else
        {
            throw new TypeNotSupportedException($"{rawParameter.ParameterName.FullName} is not supported container type.");
        }
    }

    private static readonly Action<ILogger, string, Exception?> LogTrace =
        LoggerMessage.Define<string>(LogLevel.Trace, new EventId(0), "{Message}");
}
