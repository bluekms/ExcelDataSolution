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
        PropertySchemaBase property,
        RecordSchemaContainer recordSchemaContainer,
        HashSet<RecordName> visited,
        ILogger logger)
    {
        if (property.HasAttribute<IgnoreAttribute>())
        {
            LogTrace(logger, $"{property.PropertyName.FullName} is ignored.", null);
            return;
        }

        LogTrace(logger, property.PropertyName.FullName, null);

        if (PrimitiveTypeChecker.IsSupportedPrimitiveType(property.NamedTypeSymbol))
        {
            PrimitiveTypeChecker.Check(property);
            return;
        }

        if (ContainerTypeChecker.IsSupportedContainerType(property.NamedTypeSymbol))
        {
            CheckSupportedContainerType(property, recordSchemaContainer, visited, logger);
            return;
        }

        var recordSchema = recordSchemaContainer.TryFind(property.NamedTypeSymbol);
        if (recordSchema is null)
        {
            var innerException = new KeyNotFoundException($"{property.NamedTypeSymbol.Name} is not found in the record schema dictionary.");
            throw new TypeNotSupportedException($"{property.PropertyName.FullName} is not supported record type.", innerException);
        }

        RecordTypeChecker.Check(recordSchema, recordSchemaContainer, visited, logger);
    }

    private static void CheckSupportedContainerType(
        PropertySchemaBase property,
        RecordSchemaContainer recordSchemaContainer,
        HashSet<RecordName> visited,
        ILogger logger)
    {
        if (HashSetTypeChecker.IsSupportedHashSetType(property.NamedTypeSymbol))
        {
            HashSetTypeChecker.Check(property, recordSchemaContainer, visited, logger);
        }
        else if (ListTypeChecker.IsSupportedListType(property.NamedTypeSymbol))
        {
            ListTypeChecker.Check(property, recordSchemaContainer, visited, logger);
        }
        else if (DictionaryTypeChecker.IsSupportedDictionaryType(property.NamedTypeSymbol))
        {
            DictionaryTypeChecker.Check(property, recordSchemaContainer, visited, logger);
        }
        else
        {
            throw new TypeNotSupportedException($"{property.PropertyName.FullName} is not supported container type.");
        }
    }

    private static readonly Action<ILogger, string, Exception?> LogTrace =
        LoggerMessage.Define<string>(LogLevel.Trace, new EventId(0), "{Message}");
}
