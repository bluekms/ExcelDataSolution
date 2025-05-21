using SchemaInfoScanner.Containers;

namespace SchemaInfoScanner.Schemata;

public static class RecordSchemaFactory
{
    public static RecordSchema Create(
        RecordSchema schema,
        RecordSchemaContainer recordSchemaContainer,
        IReadOnlyDictionary<string, int> headerLengths)
    {
        return new(
            schema.RecordName,
            schema.NamedTypeSymbol,
            schema.RecordAttributeList,
            schema.RecordParameterSchemaList);
    }
}
