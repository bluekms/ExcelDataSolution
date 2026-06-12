using System.Collections.Immutable;
using System.Globalization;
using System.Text;
using Sdp.Resources;

namespace Sdp.Csv;

public static class CsvLoader
{
    // CSV 행 매핑은 인터페이스 계약 대신 델리게이트로 받는다. 생성 테이블 코드가 record 의
    // 생성 매퍼(MapFromCsvRow)를 메서드 그룹으로 넘기므로 record 쪽에 별도 계약 타입이 필요 없다.
    public static async Task<ImmutableArray<TRecord>> LoadAsync<TRecord>(
        string filePath,
        Func<CsvHeaderIndex, string[], TRecord> mapFromCsvRow)
    {
        var content = await File.ReadAllTextAsync(filePath);
        return Parse(content, mapFromCsvRow, filePath);
    }

    public static ImmutableArray<TRecord> Parse<TRecord>(
        string csvContent,
        Func<CsvHeaderIndex, string[], TRecord> mapFromCsvRow,
        string? filePath = null)
    {
        var rows = ParseCsvContent(csvContent);
        if (rows.Count == 0)
        {
            return ImmutableArray<TRecord>.Empty;
        }

        var headers = new CsvHeaderIndex(rows[0], filePath);

        var builder = ImmutableArray.CreateBuilder<TRecord>(rows.Count - 1);

        for (var i = 1; i < rows.Count; i++)
        {
            var values = rows[i];
            if (values.Length == 1 && string.IsNullOrWhiteSpace(values[0]))
            {
                continue;
            }

            try
            {
                builder.Add(mapFromCsvRow(headers, values));
            }
            catch (Exception ex)
            {
                var location = filePath is not null
                    ? string.Format(
                        CultureInfo.CurrentCulture,
                        Messages.Composite.CsvRowWithFile,
                        Path.GetFileName(filePath),
                        i + 1)
                    : string.Format(
                        CultureInfo.CurrentCulture,
                        Messages.Composite.CsvRowWithoutFile,
                        i + 1);
                throw new InvalidOperationException(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        Messages.Composite.CsvRowParseError,
                        location,
                        ex.Message),
                    ex);
            }
        }

        // 빈 행을 건너뛰면 Count < Capacity 가 되어 ToImmutable() 이 배열을 다시 복사한다.
        // 용량을 실제 개수에 맞춘 뒤 MoveToImmutable() 으로 내부 버퍼를 그대로 넘긴다.
        builder.Capacity = builder.Count;
        return builder.MoveToImmutable();
    }

    private static List<string[]> ParseCsvContent(string content)
    {
        var rows = new List<string[]>();
        var fields = new List<string>();
        var field = new StringBuilder();
        var inQuotes = false;
        var i = 0;

        while (i < content.Length)
        {
            var c = content[i];

            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < content.Length && content[i + 1] == '"')
                    {
                        field.Append('"');
                        i += 2;
                    }
                    else
                    {
                        inQuotes = false;
                        i++;
                    }
                }
                else if (c == '\r')
                {
                    field.Append('\n');
                    i++;
                    if (i < content.Length && content[i] == '\n')
                    {
                        i++;
                    }
                }
                else
                {
                    field.Append(c);
                    i++;
                }
            }
            else
            {
                if (c == '"')
                {
                    inQuotes = true;
                    i++;
                }
                else if (c == ',')
                {
                    fields.Add(field.ToString());
                    field.Clear();
                    i++;
                }
                else if (c == '\r')
                {
                    fields.Add(field.ToString());
                    rows.Add(fields.ToArray());
                    fields.Clear();
                    field.Clear();
                    i++;
                    if (i < content.Length && content[i] == '\n')
                    {
                        i++;
                    }
                }
                else if (c == '\n')
                {
                    fields.Add(field.ToString());
                    rows.Add(fields.ToArray());
                    fields.Clear();
                    field.Clear();
                    i++;
                }
                else
                {
                    field.Append(c);
                    i++;
                }
            }
        }

        if (field.Length > 0 || fields.Count > 0)
        {
            fields.Add(field.ToString());
            rows.Add(fields.ToArray());
        }

        return rows;
    }
}
