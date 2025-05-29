using Microsoft.Extensions.Logging;
using SchemaInfoScanner.Catalogs;
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

        if (CatalogTypeChecker.IsSupportedCatalogType(property.NamedTypeSymbol))
        {
            CheckSupportedCatalogType(property, recordSchemaCatalog, visited, logger);
            return;
        }

        var recordSchema = recordSchemaCatalog.TryFind(property.NamedTypeSymbol);
        if (recordSchema is null)
        {
            var innerException = new KeyNotFoundException($"{property.NamedTypeSymbol.Name} is not found in the record schema dictionary.");
            throw new TypeNotSupportedException($"{property.PropertyName.FullName} is not supported record type.", innerException);
        }

        RecordTypeChecker.Check(recordSchema, recordSchemaCatalog, visited, logger);
    }

    private static void CheckSupportedCatalogType(
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
            throw new TypeNotSupportedException($"{property.PropertyName.FullName} is not supported catalog type.");
        }
    }

    private static readonly Action<ILogger, string, Exception?> LogTrace =
        LoggerMessage.Define<string>(LogLevel.Trace, new EventId(0), "{Message}");
}
