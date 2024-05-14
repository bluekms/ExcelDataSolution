using Microsoft.CodeAnalysis;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.Exceptions;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.Schemata;

namespace SchemaInfoScanner.TypeCheckers;

public static class RecordTypeChecker
{
    private static readonly string[] RecordMethodNames = { "Equals", "GetHashCode", "ToString", "PrintMembers" };

    public static bool IsSupportedRecordType(RecordSchema recordSchema)
    {
        var methodSymbols = recordSchema.NamedTypeSymbol
            .GetMembers().OfType<IMethodSymbol>()
            .Select(x => x.Name);

        return !RecordMethodNames.Except(methodSymbols).Any();
    }

    public static void Check(RecordSchema recordSchema, RecordSchemaContainer recordSchemaContainer, SemanticModelContainer semanticModelContainer, HashSet<RecordName> visited, List<string> log)
    {
        if (!IsSupportedRecordType(recordSchema))
        {
            throw new TypeNotSupportedException($"{recordSchema.RecordName.FullName} is not supported record type.");
        }

        if (!visited.Add(recordSchema.RecordName))
        {
            log.Add($"{recordSchema.RecordName.FullName} is already visited.");
            return;
        }

        foreach (var recordParameterSchema in recordSchema.RecordParameterSchemaList)
        {
            SupportedTypeChecker.Check(recordParameterSchema, recordSchemaContainer, semanticModelContainer, visited, log);
        }
    }
}
