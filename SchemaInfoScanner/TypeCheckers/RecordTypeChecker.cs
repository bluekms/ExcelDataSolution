using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.Exceptions;
using SchemaInfoScanner.Extensions;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.Schemata;
using StaticDataAttribute;

namespace SchemaInfoScanner.TypeCheckers;

internal static class RecordTypeChecker
{
    public static void Check(
        RecordSchema recordSchema,
        RecordSchemaCatalog recordSchemaCatalog,
        HashSet<RecordName> visited,
        ILogger logger)
    {
        if (recordSchema.HasAttribute<IgnoreAttribute>())
        {
            LogTrace(logger, $"{recordSchema.RecordName.FullName} is ignored.", null);
            return;
        }

        if (!IsSupportedRecordType(recordSchema.NamedTypeSymbol))
        {
            throw new InvalidOperationException($"{recordSchema.RecordName.FullName} is not supported record type.");
        }

        CheckUnavailableAttribute(recordSchema);

        if (!visited.Add(recordSchema.RecordName))
        {
            LogTrace(logger, $"{recordSchema.RecordName.FullName} is already visited.", null);
            return;
        }

        LogTrace(logger, $"{recordSchema.RecordName.FullName} Started.", null);

        foreach (var recordParameterSchema in recordSchema.RecordParameterSchemaList)
        {
            SupportedTypeChecker.Check(recordParameterSchema, recordSchemaCatalog, visited, logger);
        }

        LogTrace(logger, $"{recordSchema.RecordName.FullName} Finished.", null);
    }

    public static bool IsSupportedRecordType(INamedTypeSymbol symbol)
    {
        var methodSymbols = symbol
            .GetMembers().OfType<IMethodSymbol>()
            .Select(x => x.Name);

        return !RecordMethodNames.Except(methodSymbols).Any();
    }

    public static bool TryFindNestedRecordTypeSymbol(
        INamedTypeSymbol recordSymbol,
        ITypeSymbol propertySymbol,
        [NotNullWhen(true)] out INamedTypeSymbol? nestedRecordSymbol)
    {
        nestedRecordSymbol = null;

        if (propertySymbol is not IErrorTypeSymbol errorType)
        {
            return false;
        }

        var candidate = recordSymbol
            .GetTypeMembers()
            .FirstOrDefault(x => x.Name == errorType.Name);

        if (candidate is not null && IsSupportedRecordType(candidate))
        {
            nestedRecordSymbol = candidate;
            return nestedRecordSymbol.IsRecord;
        }

        return false;
    }

    public static RecordSchema CheckAndGetSchema(
        INamedTypeSymbol symbol,
        RecordSchemaCatalog recordSchemaCatalog,
        HashSet<RecordName> visited,
        ILogger logger)
    {
        var recordSchema = recordSchemaCatalog.TryFind(symbol);
        if (recordSchema is null)
        {
            var innerException = new KeyNotFoundException($"{symbol.Name} is not found in the RecordSchemaDictionary");
            throw new TypeNotSupportedException($"{symbol.Name} is not supported type.", innerException);
        }

        Check(recordSchema, recordSchemaCatalog, visited, logger);
        return recordSchema;
    }

    private static void CheckUnavailableAttribute(RecordSchema recordSchema)
    {
        if (recordSchema.HasAttribute<StaticDataRecordAttribute>())
        {
            throw new TypeNotSupportedException($"{nameof(StaticDataRecordAttribute)} is not available for record type {recordSchema.RecordName.FullName}. use RecordComplianceChecker.");
        }

        if (recordSchema.HasAttribute<MaxCountAttribute>())
        {
            throw new InvalidUsageException($"{nameof(MaxCountAttribute)} is not available for record type {recordSchema.RecordName.FullName}.");
        }

        if (recordSchema.HasAttribute<NullStringAttribute>())
        {
            throw new InvalidUsageException($"{nameof(NullStringAttribute)} is not available for record type {recordSchema.RecordName.FullName}.");
        }

        if (recordSchema.HasAttribute<RangeAttribute>())
        {
            throw new InvalidUsageException($"{nameof(RangeAttribute)} is not available for record type {recordSchema.RecordName.FullName}.");
        }

        if (recordSchema.HasAttribute<RegularExpressionAttribute>())
        {
            throw new InvalidUsageException($"{nameof(RegularExpressionAttribute)} is not available for record type {recordSchema.RecordName.FullName}.");
        }

        if (recordSchema.HasAttribute<SingleColumnContainerAttribute>())
        {
            throw new InvalidUsageException($"{nameof(SingleColumnContainerAttribute)} is not available for record type {recordSchema.RecordName.FullName}.");
        }
    }

    private static readonly string[] RecordMethodNames = [
        "Equals",
        "GetHashCode",
        "ToString",
        "PrintMembers"
    ];

    private static readonly Action<ILogger, string, Exception?> LogTrace =
        LoggerMessage.Define<string>(LogLevel.Trace, new EventId(0), "{Message}");
}
