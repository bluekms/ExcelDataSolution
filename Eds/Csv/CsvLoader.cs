using System.Collections.Immutable;

namespace Eds.Csv;

public static class CsvLoader
{
    public static ImmutableList<TRecord> Load<TRecord>(string filePath)
        where TRecord : class
    {
        var lines = File.ReadAllLines(filePath);
        return ParseLines<TRecord>(lines);
    }

    public static async Task<ImmutableList<TRecord>> LoadAsync<TRecord>(string filePath)
        where TRecord : class
    {
        var lines = await File.ReadAllLinesAsync(filePath);
        return ParseLines<TRecord>(lines);
    }

    public static ImmutableList<TRecord> Parse<TRecord>(string csvContent)
        where TRecord : class
    {
        var lines = csvContent.Split(["\r\n", "\n"], StringSplitOptions.None);
        return ParseLines<TRecord>(lines);
    }

    private static ImmutableList<TRecord> ParseLines<TRecord>(string[] lines)
        where TRecord : class
    {
        if (lines.Length == 0)
        {
            return ImmutableList<TRecord>.Empty;
        }

        var headers = ParseCsvLine(lines[0]);

        var builder = ImmutableList.CreateBuilder<TRecord>();
        for (var i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i]))
            {
                continue;
            }

            var values = ParseCsvLine(lines[i]);
            var record = CsvRecordMapper.MapToRecord<TRecord>(headers, values);
            builder.Add(record);
        }

        return builder.ToImmutable();
    }

    private static string[] ParseCsvLine(string line)
    {
        return line.Split(',');
    }
}
