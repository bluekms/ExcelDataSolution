using IniParser;
using IniParser.Model;
using Microsoft.Extensions.Logging;

namespace StaticDataHeaderGenerator.IniHandlers;

public static class IniSkipWriter
{
    public static void Write(string path, HashSet<RecordContainerInfo> recordContainerInfos, ILogger logger)
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
            if (File.Exists(fileName))
            {
                var oldIniData = parser.ReadFile(fileName);

                var comparisonResult = IniDataComparator.Compare(oldIniData, iniData);
                if (!comparisonResult.IsSame)
                {
                    LogWarning(logger, comparisonResult.ToString(), null);
                    continue;
                }
            }

            parser.WriteFile(fileName, iniData);
        }
    }

    private static readonly Action<ILogger, string, Exception?> LogInformation =
        LoggerMessage.Define<string>(LogLevel.Information, new EventId(0, nameof(LogInformation)), "{Message}");

    private static readonly Action<ILogger, string, Exception?> LogWarning =
        LoggerMessage.Define<string>(LogLevel.Warning, new EventId(0, nameof(LogWarning)), "{Message}");
}
