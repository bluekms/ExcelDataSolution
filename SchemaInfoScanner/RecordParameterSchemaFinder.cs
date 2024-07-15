using Microsoft.CodeAnalysis;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.Schemata;
using SchemaInfoScanner.TypeCheckers;

namespace SchemaInfoScanner;

public sealed record RecordParameterSchemaFindResult(
    RecordParameterSchema RecordParameterSchema,
    RecordParameterSchema? ParentParameterSchema);

public static class RecordParameterSchemaFinder
{
    public static List<RecordParameterSchemaFindResult> Find(this RecordSchema recordSchema, string fullName, RecordSchemaContainer recordSchemaContainer, RecordParameterSchema? parentParameterSchema = null)
    {
        var schema = recordSchema.RecordParameterSchemaList.SingleOrDefault(x => x.ParameterName.FullName == fullName);
        if (schema is not null)
        {
            return new() { new(schema, parentParameterSchema) };
        }

        var results = new List<RecordParameterSchemaFindResult>();
        foreach (var parameterSchema in recordSchema.RecordParameterSchemaList)
        {
            if (PrimitiveTypeChecker.IsSupportedPrimitiveType(parameterSchema.NamedTypeSymbol) ||
                ContainerTypeChecker.IsPrimitiveContainer(parameterSchema.NamedTypeSymbol))
            {
                continue;
            }

            if (ContainerTypeChecker.IsSupportedContainerType(parameterSchema.NamedTypeSymbol))
            {
                var typeArgument = (INamedTypeSymbol)parameterSchema.NamedTypeSymbol.TypeArguments.Single();
                var innerRecordName = new RecordName(typeArgument);
                if (recordSchemaContainer.RecordSchemaDictionary.TryGetValue(innerRecordName, out var innerRecordSchema))
                {
                    results.AddRange(innerRecordSchema.Find(fullName, recordSchemaContainer, parameterSchema));
                }

                continue;
            }

            if (RecordTypeChecker.IsSupportedRecordType(parameterSchema.NamedTypeSymbol))
            {
                var innerRecordName = new RecordName(parameterSchema.NamedTypeSymbol);
                if (recordSchemaContainer.RecordSchemaDictionary.TryGetValue(innerRecordName, out var innerRecordSchema))
                {
                    results.AddRange(innerRecordSchema.Find(fullName, recordSchemaContainer, parameterSchema));
                }

                continue;
            }
        }

        return results;
    }
}
