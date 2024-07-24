using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.Exceptions;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.Schemata;
using StaticDataAttribute;

namespace SchemaInfoScanner.TypeCheckers;

public static class RecordTypeChecker
{
    private static readonly string[] RecordMethodNames = { "Equals", "GetHashCode", "ToString", "PrintMembers" };

    private static readonly Action<ILogger, string, Exception?> LogTrace =
        LoggerMessage.Define<string>(LogLevel.Trace, new EventId(0), "{Message}");

    public static bool IsSupportedRecordType(INamedTypeSymbol symbol)
    {
        var methodSymbols = symbol
            .GetMembers().OfType<IMethodSymbol>()
            .Select(x => x.Name);

        return !RecordMethodNames.Except(methodSymbols).Any();
    }

    public static RecordSchema CheckAndGetSchema(
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

    public static void Check(
        RecordSchema recordSchema,
        RecordSchemaContainer recordSchemaContainer,
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
            SupportedTypeChecker.Check(recordParameterSchema, recordSchemaContainer, visited, logger);
        }

        LogTrace(logger, $"{recordSchema.RecordName.FullName} Finished.", null);
    }

    private static void CheckUnavailableAttribute(RecordSchema recordSchema)
    {
        if (recordSchema.HasAttribute<StaticDataRecordAttribute>())
        {
            throw new TypeNotSupportedException($"{nameof(StaticDataRecordAttribute)} is not available for record type {recordSchema.RecordName.FullName}. cannot be used as a parameter for another static data record.");
        }

        if (recordSchema.HasAttribute<RecordGlobalIndexingModeAttribute>())
        {
            throw new TypeNotSupportedException($"{nameof(RecordGlobalIndexingModeAttribute)} is not available. {recordSchema.RecordName.FullName} is not static data record.");
        }

        if (recordSchema.HasAttribute<DefaultValueAttribute>())
        {
            throw new InvalidUsageException($"{nameof(DefaultValueAttribute)} is not available for record type {recordSchema.RecordName.FullName}.");
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
}
