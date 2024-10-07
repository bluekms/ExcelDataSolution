namespace ExcelColumnExtractor.NameObjects;

public record ExcelSheetName(string ExcelPath, string SheetName)
{
    public string FullName => $"{Path.GetFileNameWithoutExtension(ExcelPath)}.{SheetName}";
}
