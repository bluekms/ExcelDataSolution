using Microsoft.Extensions.Logging;
using SchemaInfoScanner.Catalogs;
using SchemaInfoScanner.Extensions;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.Schemata;
using StaticDataAttribute;

namespace SchemaInfoScanner.TypeCheckers;

internal static class SupportedTypeChecker
{
    public static void Check(
        PropertySchemaBase property,
        RecordSchemaCatalog recordSchemaCatalog,
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

        if (CollectionTypeChecker.IsSupportedCollectionType(property.NamedTypeSymbol))
        {
            CheckSupportedCollectionType(property, recordSchemaCatalog, visited, logger);
            return;
        }

        var recordSchema = recordSchemaCatalog.TryFind(property.NamedTypeSymbol);
        if (recordSchema is null)
        {
            var innerException = new KeyNotFoundException($"{property.NamedTypeSymbol.Name} is not found in the record schema dictionary.");
            throw new NotSupportedException($"{property.PropertyName.FullName} is not supported record type.", innerException);
        }

        RecordTypeChecker.Check(recordSchema, recordSchemaCatalog, visited, logger);
    }

    private static void CheckSupportedCollectionType(
        PropertySchemaBase property,
        RecordSchemaCatalog recordSchemaCatalog,
        HashSet<RecordName> visited,
        ILogger logger)
    {
        if (HashSetTypeChecker.IsSupportedHashSetType(property.NamedTypeSymbol))
        {
            HashSetTypeChecker.Check(property, recordSchemaCatalog, visited, logger);
        }
        else if (ListTypeChecker.IsSupportedListType(property.NamedTypeSymbol))
        {
            ListTypeChecker.Check(property, recordSchemaCatalog, visited, logger);
        }
        else if (DictionaryTypeChecker.IsSupportedDictionaryType(property.NamedTypeSymbol))
        {
            DictionaryTypeChecker.Check(property, recordSchemaCatalog, visited, logger);
        }
        else
        {
            throw new NotSupportedException($"{property.PropertyName.FullName} is not supported collection type.");
        }
    }

    private static readonly Action<ILogger, string, Exception?> LogTrace =
        LoggerMessage.Define<string>(LogLevel.Trace, new EventId(0), "{Message}");
}
