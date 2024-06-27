namespace ExcelColumnExtractor.NameObjects;

public class SheetName : IEquatable<SheetName>
{
    public string ExcelPath { get; set; }

    public string Name { get; }
    public string FullName => $"{Path.GetFileNameWithoutExtension(ExcelPath)}.{Name}";

    public SheetName(string excelPath, string sheetName)
    {
        ExcelPath = excelPath;
        Name = sheetName;
    }

    public bool Equals(SheetName? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return this.FullName == other.FullName;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != this.GetType())
        {
            return false;
        }

        return Equals((SheetName)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(this.Name, this.FullName);
    }

    public override string ToString()
    {
        return FullName;
    }
}
