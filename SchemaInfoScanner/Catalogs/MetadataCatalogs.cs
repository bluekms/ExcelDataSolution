namespace SchemaInfoScanner.Catalogs;

public sealed class MetadataCatalogs(
    RecordSchemaCatalog recordSchemaCatalog,
    EnumMemberCatalog enumMemberCatalog)
{
    public RecordSchemaCatalog RecordSchemaCatalog { get; } = recordSchemaCatalog;
    public EnumMemberCatalog EnumMemberCatalog { get; } = enumMemberCatalog;
}
