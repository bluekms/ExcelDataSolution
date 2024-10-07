using Microsoft.Extensions.Logging;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.Exceptions;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.Schemata;
using SchemaInfoScanner.Schemata.RecordParameterSchemaExtensions;
using StaticDataAttribute;

namespace SchemaInfoScanner.TypeCheckers;

public static class SupportedTypeChecker
{
    private static readonly Action<ILogger, string, Exception?> LogTrace =
        LoggerMessage.Define<string>(LogLevel.Trace, new EventId(0), "{Message}");

    public static void Check(
        RecordParameterSchema recordParameter,
        RecordSchemaContainer recordSchemaContainer,
        HashSet<RecordName> visited,
        ILogger logger)
    {
        if (recordParameter.HasAttribute<IgnoreAttribute>())
        {
            LogTrace(logger, $"{recordParameter.ParameterName.FullName} is ignored.", null);
            return;
        }

        LogTrace(logger, recordParameter.ParameterName.FullName, null);

        if (PrimitiveTypeChecker.IsSupportedPrimitiveType(recordParameter))
        {
            PrimitiveTypeChecker.Check(recordParameter);
            return;
        }

        if (ContainerTypeChecker.IsSupportedContainerType(recordParameter.NamedTypeSymbol))
        {
            CheckSupportedContainerType(recordParameter, recordSchemaContainer, visited, logger);
            return;
        }

        var recordName = new RecordName(recordParameter.NamedTypeSymbol);
        if (!recordSchemaContainer.RecordSchemaDictionary.TryGetValue(recordName, out var recordSchema))
        {
            var innerException = new KeyNotFoundException($"{recordName.FullName} is not found in the record schema dictionary.");
            throw new TypeNotSupportedException($"{recordParameter.ParameterName.FullName} is not supported record type.", innerException);
        }

        RecordTypeChecker.Check(recordSchema, recordSchemaContainer, visited, logger);
    }

    private static void CheckSupportedContainerType(
        RecordParameterSchema recordParameter,
        RecordSchemaContainer recordSchemaContainer,
        HashSet<RecordName> visited,
        ILogger logger)
    {
        if (HashSetTypeChecker.IsSupportedHashSetType(recordParameter.NamedTypeSymbol))
        {
            HashSetTypeChecker.Check(recordParameter, recordSchemaContainer, visited, logger);
        }
        else if (ListTypeChecker.IsSupportedListType(recordParameter.NamedTypeSymbol))
        {
            ListTypeChecker.Check(recordParameter, recordSchemaContainer, visited, logger);
        }
        else if (DictionaryTypeChecker.IsSupportedDictionaryType(recordParameter.NamedTypeSymbol))
        {
            DictionaryTypeChecker.Check(recordParameter, recordSchemaContainer, visited, logger);
        }
        else
        {
            throw new TypeNotSupportedException($"{recordParameter.ParameterName.FullName} is not supported container type.");
        }
    }
}
