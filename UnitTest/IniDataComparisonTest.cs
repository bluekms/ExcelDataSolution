using IniParser.Parser;
using StaticDataHeaderGenerator.IniHandlers;

namespace UnitTest;

public class IniDataComparisonTest
{
    [Fact]
    public void IsSameTest()
    {
        var srcIniText = """
                         [TestExcel.TestSheet]
                         Score = 100
                         """;

        var dstIniText = """
                         [TestExcel.TestSheet]
                         Score = 100
                         """;

        var parser = new IniDataParser();
        var srcIniData = parser.Parse(srcIniText);
        var dstIniData = parser.Parse(dstIniText);

        var result = IniDataComparator.Compare(srcIniData, dstIniData);
        Assert.True(result.IsSame);
    }

    [Fact]
    public void ChangeValueTest()
    {
        var srcIniText = """
                         [TestExcel.TestSheet]
                         Score = 100
                         """;

        var dstIniText = """
                         [TestExcel.TestSheet]
                         Score = 80
                         """;

        var parser = new IniDataParser();
        var srcIniData = parser.Parse(srcIniText);
        var dstIniData = parser.Parse(dstIniText);

        var result = IniDataComparator.Compare(srcIniData, dstIniData);
        Assert.False(result.IsSame);

        var message = result.ToString();
        Assert.Contains("Score: 100 -> 80", message);
    }

    [Fact]
    public void AddKeyTest()
    {
        var srcIniText = """
                         [TestExcel.TestSheet]
                         Score = 100
                         """;

        var dstIniText = """
                         [TestExcel.TestSheet]
                         Score = 100
                         NewKey =
                         """;

        var parser = new IniDataParser();
        var srcIniData = parser.Parse(srcIniText);
        var dstIniData = parser.Parse(dstIniText);

        var result = IniDataComparator.Compare(srcIniData, dstIniData);
        Assert.False(result.IsSame);

        var message = result.ToString();
        Assert.Contains("+ NewKey", message);
    }

    [Fact]
    public void RemoveKeyTest()
    {
        var srcIniText = """
                         [TestExcel.TestSheet]
                         Score = 100
                         NewKey = 10
                         """;

        var dstIniText = """
                         [TestExcel.TestSheet]
                         Score = 100
                         """;

        var parser = new IniDataParser();
        var srcIniData = parser.Parse(srcIniText);
        var dstIniData = parser.Parse(dstIniText);

        var result = IniDataComparator.Compare(srcIniData, dstIniData);
        Assert.False(result.IsSame);

        var message = result.ToString();
        Assert.Contains("- NewKey", message);
    }

    [Fact]
    public void AddSectionTest()
    {
        var srcIniText = """
                         [TestExcel.TestSheet]
                         Score = 100
                         """;

        var dstIniText = """
                         [TestExcel.TestSheet]
                         Score = 100

                         [TestExcel.TestSheet2]
                         Values = 3
                         """;

        var parser = new IniDataParser();
        var srcIniData = parser.Parse(srcIniText);
        var dstIniData = parser.Parse(dstIniText);

        var result = IniDataComparator.Compare(srcIniData, dstIniData);
        Assert.False(result.IsSame);

        var message = result.ToString();
        Assert.Contains("+ TestExcel.TestSheet2", message);
    }

    [Fact]
    public void RemoveSectionTest()
    {
        var srcIniText = """
                         [TestExcel.TestSheet]
                         Score = 100

                         [TestExcel.TestSheet2]
                         Values = 3
                         """;

        var dstIniText = """
                         [TestExcel.TestSheet]
                         Score = 100
                         """;

        var parser = new IniDataParser();
        var srcIniData = parser.Parse(srcIniText);
        var dstIniData = parser.Parse(dstIniText);

        var result = IniDataComparator.Compare(srcIniData, dstIniData);
        Assert.False(result.IsSame);

        var message = result.ToString();
        Assert.Contains("- TestExcel.TestSheet2", message);
    }
}
