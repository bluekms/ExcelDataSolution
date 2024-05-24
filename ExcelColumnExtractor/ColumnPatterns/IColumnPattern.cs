namespace ExcelColumnExtractor.ColumnPatterns;

public interface IColumnPattern
{
    public string GetColumnHeaderPattern();

    public void CheckColumnCount();
}
