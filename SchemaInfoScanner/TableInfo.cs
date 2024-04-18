using System.Text;

namespace SchemaInfoScanner;

public sealed record TableInfo(
    string? SheetName,
    string RecordName,
    IReadOnlyList<ColumnInfo> ColumnInfoList)
{
    public override string ToString()
    {
        var sb = new StringBuilder();

        if (SheetName is null)
        {
            sb.Append(nameof(RecordName));
            sb.Append(": ");
            sb.AppendLine(RecordName);
        }
        else
        {
            sb.Append(nameof(RecordName));
            sb.Append(": ");
            sb.Append(RecordName);
            sb.Append('(');
            sb.Append(SheetName);
            sb.AppendLine(")");
        }

        foreach (var columnInfo in ColumnInfoList)
        {
            sb.Append(" - ");
            sb.AppendLine(columnInfo.ToString());
        }

        return sb.ToString();
    }
}
