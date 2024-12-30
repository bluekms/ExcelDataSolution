using IniParser;
using IniParser.Model;
using Microsoft.Extensions.Logging;

namespace StaticDataHeaderGenerator.IniHandlers;

public static class IniSkipWriter
{
    public static WriteResult Write(string path, HashSet<RecordContainerInfo> recordContainerInfos, ILogger logger)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        var parser = new FileIniDataParser();

        var writeCount = 0;
        var skipCount = 0;
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
            if (File.Exists(fileName))
            {
                var oldIniData = parser.ReadFile(fileName);

                var comparisonResult = IniDataComparator.Compare(oldIniData, iniData);
                if (comparisonResult.IsSame)
                {
                    LogInformation(logger, Path.GetFileName(fileName), null);
                    ++skipCount;
                    continue;
                }
                else
                {
                    LogWarning(logger, comparisonResult.ToString(), null);
                    ++skipCount;
                    continue;
                }
            }

            parser.WriteFile(fileName, iniData);
            ++writeCount;
        }

        return new(writeCount, skipCount);
    }

    private static readonly Action<ILogger, string, Exception?> LogInformation =
        LoggerMessage.Define<string>(LogLevel.Information, new EventId(0, nameof(LogInformation)), "{FileName} is skip.");

    private static readonly Action<ILogger, string, Exception?> LogWarning =
        LoggerMessage.Define<string>(LogLevel.Warning, new EventId(0, nameof(LogWarning)), "{Message}");
}
