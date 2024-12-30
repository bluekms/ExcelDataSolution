using IniParser;
using IniParser.Model;

namespace StaticDataHeaderGenerator.IniHandlers;

public static class IniOverwriteWriter
{
    public static void Write(string path, HashSet<RecordContainerInfo> recordContainerInfos)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        var parser = new FileIniDataParser();

        foreach (var recordContainerInfo in recordContainerInfos)
        {
            var section = new SectionData(recordContainerInfo.RecordName.FullName);
            foreach (var headerName in recordContainerInfo.LengthRequiredHeaderNames)
            {
                section.Keys.AddKey(headerName);
            }

            var iniData = new IniData();
            iniData.Sections.Add(section);

            var fileName = Path.Combine(path, $"{recordContainerInfo.RecordName.FullName}.ini");
            parser.WriteFile(fileName, iniData);
        }
    }
}
