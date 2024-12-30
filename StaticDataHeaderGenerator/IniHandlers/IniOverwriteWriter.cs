using IniParser;
using IniParser.Model;

namespace StaticDataHeaderGenerator.IniHandlers;

public static class IniOverwriteWriter
{
    public static WriteResult Write(string path, HashSet<RecordContainerInfo> recordContainerInfos)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        var parser = new FileIniDataParser();

        var writeCount = 0;
        foreach (var recordContainerInfo in recordContainerInfos)
        {
            var fileName = Path.Combine(path, $"{recordContainerInfo.RecordName.FullName}.ini");
            if (File.Exists(fileName))
            {
                continue;
            }

            var section = new SectionData(recordContainerInfo.RecordName.FullName);
            foreach (var headerName in recordContainerInfo.LengthRequiredHeaderNames)
            {
                section.Keys.AddKey(headerName);
            }

            var iniData = new IniData();
            iniData.Sections.Add(section);
            parser.WriteFile(fileName, iniData);

            ++writeCount;
        }

        return new(writeCount, 0);
    }
}
