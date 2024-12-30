namespace StaticDataHeaderGenerator.IniHandlers;

public class IniDataComparisonResultBuilder
{
    private sealed record RawModifiedSection(
        HashSet<string> AddedKeys,
        HashSet<string> RemovedKeys,
        Dictionary<string, ModifiedSectionKey> ModifiedKeys)
    {
        public static RawModifiedSection CreateEmpty() => new(new(), new(), new());
    }

    private sealed record RawIniDataComparisonResult(
        HashSet<string> AddedSections,
        HashSet<string> RemovedSections,
        Dictionary<string, RawModifiedSection> ModifiedSections)
    {
        public static RawIniDataComparisonResult CreateEmpty() => new(new(), new(), new());
    }

    private readonly RawIniDataComparisonResult rawResult = RawIniDataComparisonResult.CreateEmpty();

    public IniDataComparisonResult Build()
    {
        var addedSections = rawResult.AddedSections;
        var removedSections = rawResult.RemovedSections;
        var modifiedSections = rawResult.ModifiedSections.ToDictionary(
            kvp => kvp.Key,
            kvp => new ModifiedSection(
                kvp.Value.AddedKeys,
                kvp.Value.RemovedKeys,
                kvp.Value.ModifiedKeys));

        return new IniDataComparisonResult(addedSections, removedSections, modifiedSections);
    }

    public void AddAddedSection(string sectionName)
    {
        rawResult.AddedSections.Add(sectionName);
    }

    public void AddRemovedSection(string sectionName)
    {
        rawResult.RemovedSections.Add(sectionName);
    }

    public void AddModifiedSectionAddedKey(string sectionName, string key)
    {
        if (!rawResult.ModifiedSections.TryGetValue(sectionName, out var rawModifiedSection))
        {
            rawModifiedSection = RawModifiedSection.CreateEmpty();
            rawResult.ModifiedSections.Add(sectionName, rawModifiedSection);
        }

        rawModifiedSection.AddedKeys.Add(key);
    }

    public void AddModifiedSectionRemovedKey(string sectionName, string key)
    {
        if (!rawResult.ModifiedSections.TryGetValue(sectionName, out var rawModifiedSection))
        {
            rawModifiedSection = RawModifiedSection.CreateEmpty();
            rawResult.ModifiedSections.Add(sectionName, rawModifiedSection);
        }

        rawModifiedSection.RemovedKeys.Add(key);
    }

    public void AddModifiedSectionModifiedKey(string sectionName, string key, string srcValue, string dstValue)
    {
        if (string.IsNullOrEmpty(dstValue))
        {
            return;
        }

        if (!rawResult.ModifiedSections.TryGetValue(sectionName, out var rawModifiedSection))
        {
            rawModifiedSection = RawModifiedSection.CreateEmpty();
            rawResult.ModifiedSections.Add(sectionName, rawModifiedSection);
        }

        rawModifiedSection.ModifiedKeys.Add(key, new ModifiedSectionKey(srcValue, dstValue));
    }
}
