using Microsoft.CodeAnalysis;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.Schemata;
using SchemaInfoScanner.TypeCheckers;

namespace SchemaInfoScanner;

public sealed record RecordParameterSchemaScannerResult(
    RecordParameterSchema RecordParameterSchema,
    RecordParameterSchema? ParentParameterSchema);

public static class RecordParameterSchemaScanner
{
    public static List<RecordParameterSchemaScannerResult> Scan(
        this RecordSchema recordSchema,
        RecordSchemaContainer recordSchemaContainer,
        RecordParameterSchema? parentParameterSchema = null)
    {
        var results = new List<RecordParameterSchemaScannerResult>();

        foreach (var parameter in recordSchema.RecordParameterSchemaList)
        {
            if (PrimitiveTypeChecker.IsSupportedPrimitiveType(parameter.NamedTypeSymbol) ||
                ContainerTypeChecker.IsPrimitiveContainer(parameter.NamedTypeSymbol))
            {
                results.Add(new(parameter, parentParameterSchema));
            }

            if (ContainerTypeChecker.IsSupportedContainerType(parameter.NamedTypeSymbol))
            {
                var typeArgument = (INamedTypeSymbol)parameter.NamedTypeSymbol.TypeArguments.Single();
                var innerRecordName = new RecordName(typeArgument);
                if (recordSchemaContainer.RecordSchemaDictionary.TryGetValue(innerRecordName, out var innerRecordSchema))
                {
                    results.AddRange(innerRecordSchema.Scan(recordSchemaContainer, parameter));
                }

                continue;
            }

            if (RecordTypeChecker.IsSupportedRecordType(parameter.NamedTypeSymbol))
            {
                var innerRecordName = new RecordName(parameter.NamedTypeSymbol);
                if (recordSchemaContainer.RecordSchemaDictionary.TryGetValue(innerRecordName, out var innerRecordSchema))
                {
                    results.AddRange(innerRecordSchema.Scan(recordSchemaContainer, parameter));
                }

                continue;
            }
        }

        return results;
    }
}
