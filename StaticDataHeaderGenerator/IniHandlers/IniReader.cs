using System.Collections.Frozen;
using System.Data;
using IniParser;
using SchemaInfoScanner.NameObjects;

namespace StaticDataHeaderGenerator.IniHandlers;

public sealed record IniFileResult(RecordName RecordName, FrozenDictionary<string, int> HeaderNameLengths);

public static class IniReader
{
    public static Dictionary<RecordName, IniFileResult> Read(string path, RecordContainerInfo recordContainerInfo)
    {
        var parser = new FileIniDataParser();
        var fileName = Path.Combine(path, $"{recordContainerInfo.RecordName}.ini");
        if (!File.Exists(fileName))
        {
            throw new FileNotFoundException($"File {fileName} not found. Please use 'length' or 'all-length' command to generate the ini file.");
        }

        var iniData = parser.ReadFile(fileName);

        var results = new Dictionary<RecordName, IniFileResult>();
        foreach (var section in iniData.Sections)
        {
            var headerNameLengths = new Dictionary<string, int>();
            foreach (var key in section.Keys)
            {
                if (!int.TryParse(key.Value, out var value))
                {
                    if (string.IsNullOrEmpty(key.Value))
                    {
                        throw new FormatException($"{fileName}'s {section.SectionName}.{key.KeyName} Value is empty.");
                    }
                    else
                    {
                        throw new FormatException($"{fileName}'s {section.SectionName}.{key.KeyName} Value {key.Value} is not a valid integer.");
                    }
                }

                headerNameLengths[key.KeyName] = value;
            }

            if (headerNameLengths.Count != recordContainerInfo.LengthRequiredHeaderNames.Count)
            {
                throw new DataException("The ini file and the record version are different. Please recreate the length ini file.");
            }

            results[recordContainerInfo.RecordName] = new(recordContainerInfo.RecordName, headerNameLengths.ToFrozenDictionary());
        }

        return results;
    }
}
