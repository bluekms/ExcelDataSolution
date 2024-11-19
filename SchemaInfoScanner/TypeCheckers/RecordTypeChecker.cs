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
        RawRecordSchema rawRecordSchema,
        RecordSchemaContainer recordSchemaContainer,
        HashSet<RecordName> visited,
        ILogger logger)
    {
        if (rawRecordSchema.HasAttribute<IgnoreAttribute>())
        {
            LogTrace(logger, $"{rawRecordSchema.RecordName.FullName} is ignored.", null);
            return;
        }

        if (!IsSupportedRecordType(rawRecordSchema.NamedTypeSymbol))
        {
            throw new InvalidOperationException($"{rawRecordSchema.RecordName.FullName} is not supported record type.");
        }

        CheckUnavailableAttribute(rawRecordSchema);

        if (!visited.Add(rawRecordSchema.RecordName))
        {
            LogTrace(logger, $"{rawRecordSchema.RecordName.FullName} is already visited.", null);
            return;
        }

        LogTrace(logger, $"{rawRecordSchema.RecordName.FullName} Started.", null);

        foreach (var recordParameterSchema in rawRecordSchema.RawParameterSchemaList)
        {
            SupportedTypeChecker.Check(recordParameterSchema, recordSchemaContainer, visited, logger);
        }

        LogTrace(logger, $"{rawRecordSchema.RecordName.FullName} Finished.", null);
    }

    public static bool IsSupportedRecordType(INamedTypeSymbol symbol)
    {
        var methodSymbols = symbol
            .GetMembers().OfType<IMethodSymbol>()
            .Select(x => x.Name);

        return !RecordMethodNames.Except(methodSymbols).Any();
    }

    public static RawRecordSchema CheckAndGetSchema(
        INamedTypeSymbol symbol,
        RecordSchemaContainer recordSchemaContainer,
        HashSet<RecordName> visited,
        ILogger logger)
    {
        var recordName = new RecordName(symbol);
        if (!recordSchemaContainer.RecordSchemaDictionary.TryGetValue(recordName, out var recordSchema))
        {
            var innerException = new KeyNotFoundException($"{recordName.FullName} is not found in the RecordSchemaDictionary");
            throw new TypeNotSupportedException($"{recordName.FullName} is not supported type.", innerException);
        }

        Check(recordSchema, recordSchemaContainer, visited, logger);

        return recordSchema;
    }

    private static void CheckUnavailableAttribute(RawRecordSchema rawRecordSchema)
    {
        if (rawRecordSchema.HasAttribute<StaticDataRecordAttribute>())
        {
            throw new TypeNotSupportedException($"{nameof(StaticDataRecordAttribute)} is not available for record type {rawRecordSchema.RecordName.FullName}. cannot be used as a parameter for another static data record.");
        }

        if (rawRecordSchema.HasAttribute<MaxCountAttribute>())
        {
            throw new InvalidUsageException($"{nameof(MaxCountAttribute)} is not available for record type {rawRecordSchema.RecordName.FullName}.");
        }

        if (rawRecordSchema.HasAttribute<NullStringAttribute>())
        {
            throw new InvalidUsageException($"{nameof(NullStringAttribute)} is not available for record type {rawRecordSchema.RecordName.FullName}.");
        }

        if (rawRecordSchema.HasAttribute<RangeAttribute>())
        {
            throw new InvalidUsageException($"{nameof(RangeAttribute)} is not available for record type {rawRecordSchema.RecordName.FullName}.");
        }

        if (rawRecordSchema.HasAttribute<RegularExpressionAttribute>())
        {
            throw new InvalidUsageException($"{nameof(RegularExpressionAttribute)} is not available for record type {rawRecordSchema.RecordName.FullName}.");
        }

        if (rawRecordSchema.HasAttribute<SingleColumnContainerAttribute>())
        {
            throw new InvalidUsageException($"{nameof(SingleColumnContainerAttribute)} is not available for record type {rawRecordSchema.RecordName.FullName}.");
        }
    }

    private static readonly string[] RecordMethodNames = { "Equals", "GetHashCode", "ToString", "PrintMembers" };

    private static readonly Action<ILogger, string, Exception?> LogTrace =
        LoggerMessage.Define<string>(LogLevel.Trace, new EventId(0), "{Message}");
}
