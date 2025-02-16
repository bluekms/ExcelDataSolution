namespace StaticDataAttribute;

[AttributeUsage(AttributeTargets.Class)]
public sealed class StaticDataRecordAttribute : Attribute
{
    public string ExcelFileName { get; }
    public string SheetName { get; }

    public StaticDataRecordAttribute(string excelFileName, string sheetName)
    {
        ExcelFileName = excelFileName;
        SheetName = sheetName;
    }
}
