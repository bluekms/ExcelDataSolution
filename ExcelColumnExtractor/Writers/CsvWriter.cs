using System.Text;
using ExcelColumnExtractor.Aggregator;
using ExcelColumnExtractor.Containers;

namespace ExcelColumnExtractor.Writers;

public static class CsvWriter
{
    private const int FlushThreshold = 1024 * 1024;
    private static readonly char[] SpecialChars = [',', '"', '\n', '\r'];

    public static void Write(
        string path,
        Encoding encoding,
        ExtractedTableContainer extractedTableContainer)
    {
        foreach (var (recordSchema, table) in extractedTableContainer.SortedTables)
        {
            var fileName = Path.Combine(path, $"{recordSchema.RecordName.FullName}.csv");
            var sb = new StringBuilder();
            sb.AppendLine(string.Join(",", table.Headers));

            using var writer = new StreamWriter(fileName, false, encoding);
            foreach (var row in table.Rows)
            {
                if (sb.Length > FlushThreshold)
                {
                    writer.Write(sb.ToString());
                    sb.Clear();
                }

                sb.AppendLine(ConvertRowToCsv(row));
            }

            if (sb.Length > 0)
            {
                writer.Write(sb.ToString());
            }
        }
    }

    private static string ConvertRowToCsv(BodyColumnAggregator.ExtractedRow row)
    {
        var sb = new StringBuilder();
        foreach (var cell in row.Data)
        {
            if (cell.IndexOfAny(SpecialChars) != -1)
            {
                sb.Append('"');
                sb.Append(cell.Replace("\"", "\"\""));
                sb.Append('"');
            }
            else
            {
                sb.Append(cell);
            }

            sb.Append(',');
        }

        if (sb.Length > 0)
        {
            --sb.Length;
        }

        return sb.ToString();
    }
}
