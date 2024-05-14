using SchemaInfoScanner.Containers;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.TypeCheckers;
using StaticDataAttribute;

namespace SchemaInfoScanner;

public static class Checker
{
    public static void Check(RecordSchemaContainer recordSchemaContainer, SemanticModelContainer semanticModelContainer, List<string> log)
    {
        var visited = new HashSet<RecordName>();
        foreach (var (_, recordSchema) in recordSchemaContainer.RecordSchemaDictionary)
        {
            if (!recordSchema.HasAttribute<StaticDataRecordAttribute>())
            {
                continue;
            }

            if (!visited.Add(recordSchema.RecordName))
            {
                log.Add($"{recordSchema.RecordName.FullName} is already visited.");
                continue;
            }

            foreach (var recordParameter in recordSchema.RecordParameterSchemaList)
            {
                SupportedTypeChecker.Check(recordParameter, recordSchemaContainer, semanticModelContainer, visited, log);
            }
        }
    }
}
