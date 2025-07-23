using SchemaInfoScanner.Catalogs;

namespace SchemaInfoScanner.Schemata;

public static class RecordSchemaFactory
{
    public static RecordSchema Create(
        RecordSchema schema,
        RecordSchemaCatalog recordSchemaCatalog,
        IReadOnlyDictionary<string, int> headerLengths)
    {
        return new(
            schema.RecordName,
            schema.NamedTypeSymbol,
            schema.RecordAttributeList,
            schema.PropertySchemata);
    }
}
