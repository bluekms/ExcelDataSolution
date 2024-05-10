using Microsoft.CodeAnalysis;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.Exceptions;
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

    public static void Check(RecordSchema recordSchema, RecordSchemaContainer recordSchemaContainer, SemanticModelContainer semanticModelContainer)
    {
        if (!IsSupportedRecordType(recordSchema))
        {
            throw new TypeNotSupportedException($"{recordSchema.RecordName.FullName} is not supported record type.");
        }

        foreach (var recordParameterSchema in recordSchema.RecordParameterSchemaList)
        {
            SupportedTypeChecker.Check(recordParameterSchema, recordSchemaContainer, semanticModelContainer);
        }
    }
}
