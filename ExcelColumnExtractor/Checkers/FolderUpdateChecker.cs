using System.Globalization;
using ExcelColumnExtractor.Scanners;
using Microsoft.Extensions.Logging;

namespace ExcelColumnExtractor.Checkers;

public static class FolderUpdateChecker
{
    private static string DateTimeFormat => "yyyy-MM-dd HH:mm:ss";

    public static void Check(FolderState before, FolderState after, ILogger logger)
    {
        if (before.FolderPath != after.FolderPath)
        {
            throw new ArgumentException($"The folder paths are different. {before.FolderPath} != {after.FolderPath}");
        }

        var added = after.FileStates.Keys.Except(before.FileStates.Keys).ToList();
        foreach (var path in added)
        {
            LogWarning(logger, $"File {path} was added.", null);
        }

        var removed = before.FileStates.Keys.Except(after.FileStates.Keys).ToList();
        foreach (var path in removed)
        {
            LogWarning(logger, $"File {path} was removed.", null);
        }

        foreach (var file in before.FileStates.Keys.Intersect(after.FileStates.Keys))
        {
            var beforeTime = before.FileStates[file];
            var afterTime = after.FileStates[file];

            if (beforeTime != afterTime)
            {
                if (afterTime < beforeTime)
                {
                    throw new InvalidOperationException($"File {file} was updated before the last capture. {beforeTime.ToString(DateTimeFormat, CultureInfo.InvariantCulture)} -> {afterTime.ToString(DateTimeFormat, CultureInfo.InvariantCulture)}");
                }

                LogWarning(logger, $"File {file} was updated after the last capture.", null);
            }
        }
    }

    private static readonly Action<ILogger, string, Exception?> LogWarning =
        LoggerMessage.Define<string>(LogLevel.Warning, new EventId(0, nameof(LogWarning)), "{Message}");
}
