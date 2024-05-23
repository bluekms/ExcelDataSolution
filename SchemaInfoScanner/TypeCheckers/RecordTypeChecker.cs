using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.Exceptions;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.Schemata;

namespace SchemaInfoScanner.TypeCheckers;

public static class RecordTypeChecker
{
    private static readonly string[] RecordMethodNames = { "Equals", "GetHashCode", "ToString", "PrintMembers" };

    private static readonly Action<ILogger, string, Exception?> LogTrace =
        LoggerMessage.Define<string>(LogLevel.Trace, new EventId(0), "{Message}");

    public static bool IsSupportedRecordType(RecordSchema recordSchema)
    {
        var methodSymbols = recordSchema.NamedTypeSymbol
            .GetMembers().OfType<IMethodSymbol>()
            .Select(x => x.Name);

        return !RecordMethodNames.Except(methodSymbols).Any();
    }

    public static void Check(
        RecordSchema recordSchema,
        RecordSchemaContainer recordSchemaContainer,
        HashSet<RecordName> visited,
        ILogger logger)
    {
        if (!IsSupportedRecordType(recordSchema))
        {
            throw new TypeNotSupportedException($"{recordSchema.RecordName.FullName} is not supported record type.");
        }

        if (!visited.Add(recordSchema.RecordName))
        {
            LogTrace(logger, $"{recordSchema.RecordName.FullName} is already visited.", null);
            return;
        }

        foreach (var recordParameterSchema in recordSchema.RecordParameterSchemaList)
        {
            SupportedTypeChecker.Check(recordParameterSchema, recordSchemaContainer, visited, logger);
        }
    }
}
