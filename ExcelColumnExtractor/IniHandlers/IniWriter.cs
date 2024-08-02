namespace ExcelColumnExtractor.IniHandlers;

public sealed record RecordContainerInfo(string RecordName, HashSet<string> LengthRequiredHeaderNames);

public static class IniWriter
{
    public static void Write(string path, HashSet<RecordContainerInfo> recordContainerInfos)
    {
        if (!string.IsNullOrEmpty(Path.GetFileName(path)))
        {
            throw new ArgumentException($"Remove file name from path: {path}");
        }

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        var parser = new IniParser.FileIniDataParser();
        var iniData = new IniParser.Model.IniData();

        foreach (var recordContainerInfo in recordContainerInfos)
        {
            var section = new IniParser.Model.SectionData(recordContainerInfo.RecordName);
            foreach (var headerName in recordContainerInfo.LengthRequiredHeaderNames)
            {
                section.Keys.AddKey(headerName);
            }

            iniData.Sections.Add(section);
            parser.WriteFile(path, iniData);
        }
    }
}
