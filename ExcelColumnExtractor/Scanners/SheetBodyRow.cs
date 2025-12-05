using SchemaInfoScanner.Schemata.TypedPropertySchemata;

namespace ExcelColumnExtractor.Scanners;

public sealed record SheetBodyRow(IReadOnlyList<CellData> Cells);
