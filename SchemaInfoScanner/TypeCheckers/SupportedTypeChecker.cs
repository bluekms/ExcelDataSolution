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
        ParameterSchemaBase parameter,
        RecordSchemaContainer recordSchemaContainer,
        HashSet<RecordName> visited,
        ILogger logger)
    {
        if (parameter.HasAttribute<IgnoreAttribute>())
        {
            LogTrace(logger, $"{parameter.ParameterName.FullName} is ignored.", null);
            return;
        }

        LogTrace(logger, parameter.ParameterName.FullName, null);

        if (PrimitiveTypeChecker.IsSupportedPrimitiveType(parameter.NamedTypeSymbol))
        {
            PrimitiveTypeChecker.Check(parameter);
            return;
        }

        if (ContainerTypeChecker.IsSupportedContainerType(parameter.NamedTypeSymbol))
        {
            CheckSupportedContainerType(parameter, recordSchemaContainer, visited, logger);
            return;
        }

        var recordSchema = recordSchemaContainer.TryFind(parameter.NamedTypeSymbol);
        if (recordSchema is null)
        {
            var innerException = new KeyNotFoundException($"{parameter.NamedTypeSymbol.Name} is not found in the record schema dictionary.");
            throw new TypeNotSupportedException($"{parameter.ParameterName.FullName} is not supported record type.", innerException);
        }

        RecordTypeChecker.Check(recordSchema, recordSchemaContainer, visited, logger);
    }

    private static void CheckSupportedContainerType(
        ParameterSchemaBase parameter,
        RecordSchemaContainer recordSchemaContainer,
        HashSet<RecordName> visited,
        ILogger logger)
    {
        if (HashSetTypeChecker.IsSupportedHashSetType(parameter.NamedTypeSymbol))
        {
            HashSetTypeChecker.Check(parameter, recordSchemaContainer, visited, logger);
        }
        else if (ListTypeChecker.IsSupportedListType(parameter.NamedTypeSymbol))
        {
            ListTypeChecker.Check(parameter, recordSchemaContainer, visited, logger);
        }
        else if (DictionaryTypeChecker.IsSupportedDictionaryType(parameter.NamedTypeSymbol))
        {
            DictionaryTypeChecker.Check(parameter, recordSchemaContainer, visited, logger);
        }
        else
        {
            throw new TypeNotSupportedException($"{parameter.ParameterName.FullName} is not supported container type.");
        }
    }

    private static readonly Action<ILogger, string, Exception?> LogTrace =
        LoggerMessage.Define<string>(LogLevel.Trace, new EventId(0), "{Message}");
}
