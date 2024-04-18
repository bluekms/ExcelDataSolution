namespace SchemaInfoScanner;

public sealed record SchemaInfo(
    string Version,
    IReadOnlyList<TableInfo> TableInfoList);
