namespace Sdp.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public sealed class StaticDataRecordAttribute(
    string excelFileName,
    string sheetName,
    string? startCell = null)
    : Attribute
{
    public string ExcelFileName { get; } = excelFileName;
    public string SheetName { get; } = sheetName;
    public string? StartCell { get; } = startCell;
}
