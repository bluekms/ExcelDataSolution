using Microsoft.CodeAnalysis;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.Schemata;
using SchemaInfoScanner.TypeCheckers;
using StaticDataAttribute;

namespace SchemaInfoScanner.Extensions;

public static class RecordSchemaLengthRequiringFieldDetector
{
    public static HashSet<string> DetectLengthRequiringFields(
        this RawRecordSchema rawRecordSchema,
        RecordSchemaContainer recordSchemaContainer,
        string parentPrefix = "")
    {
        var results = new HashSet<string>();
        foreach (var parameter in rawRecordSchema.RecordParameterSchemaList)
        {
            var name = parameter.ParameterName.Name;
            if (parameter.TryGetAttributeValue<ColumnNameAttribute, string>(0, out var columnName))
            {
                name = columnName;
            }

            var headerName = string.IsNullOrEmpty(parentPrefix)
                ? name
                : $"{parentPrefix}.{name}";

            if (ContainerTypeChecker.IsPrimitiveContainer(parameter.NamedTypeSymbol))
            {
                if (!parameter.HasAttribute<SingleColumnContainerAttribute>())
                {
                    results.Add(headerName);
                }
            }
            else if (DictionaryTypeChecker.IsSupportedDictionaryType(parameter.NamedTypeSymbol))
            {
                results.Add(headerName);
                var typeArgument = (INamedTypeSymbol)parameter.NamedTypeSymbol.TypeArguments.Last();
                var innerRecordName = new RecordName(typeArgument);
                if (recordSchemaContainer.RecordSchemaDictionary.TryGetValue(innerRecordName, out var innerRecordSchema))
                {
                    var innerCollectionNames = innerRecordSchema.DetectLengthRequiringFields(recordSchemaContainer, headerName);
                    foreach (var innerName in innerCollectionNames)
                    {
                        results.Add(innerName);
                    }
                }
            }
            else if (ContainerTypeChecker.IsSupportedContainerType(parameter.NamedTypeSymbol))
            {
                results.Add(headerName);
                var typeArgument = (INamedTypeSymbol)parameter.NamedTypeSymbol.TypeArguments.Single();
                var innerRecordName = new RecordName(typeArgument);
                if (recordSchemaContainer.RecordSchemaDictionary.TryGetValue(innerRecordName, out var innerRecordSchema))
                {
                    var innerCollectionNames = innerRecordSchema.DetectLengthRequiringFields(recordSchemaContainer, headerName);
                    foreach (var innerName in innerCollectionNames)
                    {
                        results.Add(innerName);
                    }
                }
            }
            else
            {
                var innerRecordName = new RecordName(parameter.NamedTypeSymbol);
                if (recordSchemaContainer.RecordSchemaDictionary.TryGetValue(innerRecordName, out var innerRecordSchema))
                {
                    var innerCollectionNames = innerRecordSchema.DetectLengthRequiringFields(recordSchemaContainer, headerName);
                    foreach (var innerName in innerCollectionNames)
                    {
                        results.Add(innerName);
                    }
                }
            }
        }

        return results;
    }
}
