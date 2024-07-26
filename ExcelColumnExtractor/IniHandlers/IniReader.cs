using System.Data;
using IniParser;

namespace ExcelColumnExtractor.IniHandlers;

public static class IniReader
{
    public static Dictionary<string, Dictionary<string, int>> Read(string path, HashSet<RecordContainerInfo> recordContainerInfos)
    {
        var parser = new FileIniDataParser();

        var results = new Dictionary<string, Dictionary<string, int>>();
        foreach (var recordContainerInfo in recordContainerInfos)
        {
            var fileName = Path.Combine(path, $"{recordContainerInfo.RecordName}.ini");
            var iniData = parser.ReadFile(fileName);
            foreach (var section in iniData.Sections)
            {
                var headerNameLengths = new Dictionary<string, int>();
                foreach (var key in section.Keys)
                {
                    if (!int.TryParse(key.Value, out var value))
                    {
                        throw new FormatException($"Value {key.Value} is not a valid integer.");
                    }

                    headerNameLengths[key.KeyName] = value;
                }

                if (headerNameLengths.Count != recordContainerInfo.LengthRequiredHeaderNames.Count)
                {
                    throw new DataException("The ini file and the record version are different. Please recreate the length ini file.");
                }

                results[section.SectionName] = headerNameLengths;
            }
        }

        return results;
    }
}
