namespace ExcelColumnExtractor.NameObject;

public sealed class SheetName
{
    public string Name { get; }
    public string ExcelPath { get; }

    public SheetName(string name, string excelPath)
    {
        Name = name;
        ExcelPath = excelPath;
    }
}
