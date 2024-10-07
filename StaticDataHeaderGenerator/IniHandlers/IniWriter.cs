using SchemaInfoScanner.NameObjects;

namespace StaticDataHeaderGenerator.IniHandlers;

public sealed record RecordContainerInfo(RecordName RecordName, HashSet<string> LengthRequiredHeaderNames);

public static class IniWriter
{
    public static void Write(string path, HashSet<RecordContainerInfo> recordContainerInfos)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        var parser = new IniParser.FileIniDataParser();

        foreach (var recordContainerInfo in recordContainerInfos)
        {
            var section = new IniParser.Model.SectionData(recordContainerInfo.RecordName.FullName);
            foreach (var headerName in recordContainerInfo.LengthRequiredHeaderNames)
            {
                section.Keys.AddKey(headerName);
            }

            var iniData = new IniParser.Model.IniData();
            iniData.Sections.Add(section);

            var fileName = Path.Combine(path, $"{recordContainerInfo.RecordName.FullName}.ini");
            parser.WriteFile(fileName, iniData);
        }
    }
}
