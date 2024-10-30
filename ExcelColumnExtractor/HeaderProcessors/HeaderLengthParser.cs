using System.Globalization;
using System.Text.RegularExpressions;

namespace ExcelColumnExtractor.HeaderProcessors;

public static class HeaderLengthParser
{
    public static IReadOnlyDictionary<string, int> Parse(IReadOnlyList<string> sheetHeaders, HashSet<string> lengthRequiredNames)
    {
        var srcNames = lengthRequiredNames.ToList();
        var patterns = lengthRequiredNames.ToList();
        for (var i = 0; i < patterns.Count; ++i)
        {
            ConvertNamesToPatterns(patterns[i], patterns);
        }

        if (srcNames.Count != patterns.Count)
        {
            throw new InvalidOperationException("Failed to convert names to regex.");
        }

        var result = new Dictionary<string, int>();
        for (var i = 0; i < srcNames.Count; ++i)
        {
            var key = srcNames[i];
            var value = GetMaxCapturedValue(sheetHeaders, patterns[i]);
            result[key] = value;
        }

        return result.AsReadOnly();
    }

    private static void ConvertNamesToPatterns(string targetName, List<string> lengthRequiredNames)
    {
        for (var i = 0; i < lengthRequiredNames.Count; ++i)
        {
            var name = lengthRequiredNames[i];
            if (!name.StartsWith(targetName, StringComparison.Ordinal))
            {
                continue;
            }

            if (name.Length == targetName.Length)
            {
                lengthRequiredNames[i] = CreateTerminalRegexPattern(targetName);
            }
            else if (name[targetName.Length] == '.')
            {
                lengthRequiredNames[i] = name.Replace(targetName, CreateRegexPattern(targetName));
            }
        }
    }

    private static int GetMaxCapturedValue(IReadOnlyList<string> sheetHeaders, string pattern)
    {
        var maxNumber = 0;
        foreach (var header in sheetHeaders)
        {
            var match = Regex.Match(header, pattern);
            if (match.Success)
            {
                maxNumber = Math.Max(maxNumber, int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture));
            }
        }

        return maxNumber + 1;
    }

    private static string CreateTerminalRegexPattern(string name)
    {
        return $"{name}\\[(\\d+)\\]";
    }

    private static string CreateRegexPattern(string name)
    {
        return $"{name}\\[\\d+\\]";
    }
}
