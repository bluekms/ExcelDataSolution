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
    public static List<RecordParameterSchemaFindResult> Find(
        this RecordSchema recordSchema,
        string fullName,
        RecordSchemaContainer recordSchemaContainer,
        RecordParameterSchema? parentParameterSchema = null)
    {
        var schema = recordSchema.RecordParameterSchemaList.SingleOrDefault(x => x.ParameterName.FullName == fullName);
        if (schema is not null)
        {
            return new() { new(schema, parentParameterSchema) };
        }

        var results = new List<RecordParameterSchemaFindResult>();
        foreach (var parameter in recordSchema.RecordParameterSchemaList)
        {
            if (PrimitiveTypeChecker.IsSupportedPrimitiveType(parameter.NamedTypeSymbol) ||
                ContainerTypeChecker.IsPrimitiveContainer(parameter.NamedTypeSymbol))
            {
                continue;
            }

            if (ContainerTypeChecker.IsSupportedContainerType(parameter.NamedTypeSymbol))
            {
                var typeArgument = (INamedTypeSymbol)parameter.NamedTypeSymbol.TypeArguments.Single();
                var innerRecordName = new RecordName(typeArgument);
                if (recordSchemaContainer.RecordSchemaDictionary.TryGetValue(innerRecordName, out var innerRecordSchema))
                {
                    results.AddRange(innerRecordSchema.Find(fullName, recordSchemaContainer, parameter));
                }

                continue;
            }

            if (RecordTypeChecker.IsSupportedRecordType(parameter.NamedTypeSymbol))
            {
                var innerRecordName = new RecordName(parameter.NamedTypeSymbol);
                if (recordSchemaContainer.RecordSchemaDictionary.TryGetValue(innerRecordName, out var innerRecordSchema))
                {
                    results.AddRange(innerRecordSchema.Find(fullName, recordSchemaContainer, parameter));
                }

                continue;
            }
        }

        return results;
    }
}
