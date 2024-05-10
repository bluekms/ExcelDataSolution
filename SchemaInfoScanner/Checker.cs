using SchemaInfoScanner.Containers;
using SchemaInfoScanner.TypeCheckers;
using StaticDataAttribute;

namespace SchemaInfoScanner;

public static class Checker
{
    public static void Check(RecordSchemaContainer recordSchemaContainer, SemanticModelContainer semanticModelContainer)
    {
        foreach (var (_, recordSchema) in recordSchemaContainer.RecordSchemaDictionary)
        {
            if (!recordSchema.HasAttribute<StaticDataRecordAttribute>())
            {
                continue;
            }

            foreach (var recordParameter in recordSchema.RecordParameterSchemaList)
            {
                SupportedTypeChecker.Check(recordParameter, recordSchemaContainer, semanticModelContainer);
            }
        }
    }
}
