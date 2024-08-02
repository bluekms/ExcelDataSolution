using IniParser;

namespace ExcelColumnExtractor.IniHandlers;

public static class IniReader
{
    public sealed record RecordContainerLengthInfo(string RecordName, Dictionary<string, int?> HeaderNameLengths);

    public static List<RecordContainerLengthInfo> Read(string path)
    {
        var parser = new FileIniDataParser();
        var iniData = parser.ReadFile(path);

        var results = new List<RecordContainerLengthInfo>();
        foreach (var section in iniData.Sections)
        {
            var headerNameLengths = new Dictionary<string, int?>();
            foreach (var key in section.Keys)
            {
                headerNameLengths[key.KeyName] = int.TryParse(key.Value, out var value) ? value : null;
            }

            results.Add(new RecordContainerLengthInfo(section.SectionName, headerNameLengths));
        }

        return results;
    }
}
