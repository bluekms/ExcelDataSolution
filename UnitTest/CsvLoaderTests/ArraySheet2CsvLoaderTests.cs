using System.Collections.Immutable;
using Eds.Attributes;
using Eds.Csv;

namespace UnitTest.CsvLoaderTests;

public class ArraySheet2CsvLoaderTests
{
    [StaticDataRecord("Excel1", "ArraySheet")]
    public sealed record ArraySheet2(
        string Name,

        [ColumnName("Score")]
        [Length(3)]
        ImmutableArray<int> Scores);

    [Fact]
    public void Load_ArraySheet2Csv_ReturnsValidRecords()
    {
        var csvPath = GetCsvPath("Excel1.ArraySheet2.csv");

        var records = CsvLoader.Load<ArraySheet2>(csvPath);

        Assert.NotEmpty(records);
        Assert.Equal(9, records.Count);

        // 첫 번째 레코드 검증 (멀티라인 + 특수문자 포함)
        Assert.Equal("동해물과\r\n백두산이, 마르고 닳도록　~.", records[0].Name);
        Assert.Equal(3, records[0].Scores.Length);
        Assert.Equal(111, records[0].Scores[0]);
        Assert.Equal(222, records[0].Scores[1]);
        Assert.Equal(333, records[0].Scores[2]);

        // 두 번째 레코드 검증 (일본어)
        Assert.Equal("山田太郎", records[1].Name);
        Assert.Equal(77, records[1].Scores[0]);
        Assert.Equal(84, records[1].Scores[1]);
        Assert.Equal(80, records[1].Scores[2]);

        // 한국어 레코드 검증
        var koreanRecord = records.First(r => r.Name == "이영희");
        Assert.Equal(94, koreanRecord.Scores[0]);
        Assert.Equal(97, koreanRecord.Scores[1]);
        Assert.Equal(93, koreanRecord.Scores[2]);
    }

    [Fact]
    public void Load_ArraySheet2Csv_AllRecordsHaveThreeScores()
    {
        var csvPath = GetCsvPath("Excel1.ArraySheet2.csv");

        var records = CsvLoader.Load<ArraySheet2>(csvPath);

        foreach (var record in records)
        {
            Assert.Equal(3, record.Scores.Length);
        }
    }

    private static string GetCsvPath(string fileName)
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var solutionDir = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", ".."));
        return Path.Combine(solutionDir, "Docs", "TestCsv", fileName);
    }
}
