using IniParser.Model;

namespace StaticDataHeaderGenerator.IniHandlers;

public class IniDataComparator
{
    public static IniDataComparisonResult Compare(IniData srcData, IniData dstData)
    {
        var resultBuilder = new IniDataComparisonResultBuilder();

        var srcSectionNames = srcData.Sections.Select(x => x.SectionName).ToHashSet();
        var dstSectionNames = dstData.Sections.Select(x => x.SectionName).ToHashSet();

        var commonSectionNames = srcSectionNames.Intersect(dstSectionNames).ToHashSet();
        foreach (var sectionName in commonSectionNames)
        {
            var srcSectionKeyNames = srcData[sectionName].Select(x => x.KeyName).ToList();
            var dstSectionKeyNames = dstData[sectionName].Select(x => x.KeyName).ToList();

            var commonKeyNames = srcSectionKeyNames.Intersect(dstSectionKeyNames).ToList();
            foreach (var key in commonKeyNames)
            {
                var srcValue = srcData[sectionName][key];
                var dstValue = dstData[sectionName][key];
                if (srcValue != dstValue)
                {
                    resultBuilder.AddModifiedSectionModifiedKey(sectionName, key, srcValue, dstValue);
                }
            }

            var addedKeyNames = dstSectionKeyNames.Except(srcSectionKeyNames).ToList();
            foreach (var addedItem in addedKeyNames)
            {
                resultBuilder.AddModifiedSectionAddedKey(sectionName, addedItem);
            }

            var removedKeyNames = srcSectionKeyNames.Except(dstSectionKeyNames).ToList();
            foreach (var removedItem in removedKeyNames)
            {
                resultBuilder.AddModifiedSectionRemovedKey(sectionName, removedItem);
            }
        }

        var addedSectionNames = dstSectionNames.Except(srcSectionNames).ToList();
        foreach (var addSectionName in addedSectionNames)
        {
            resultBuilder.AddAddedSection(addSectionName);
        }

        var removedSectionNames = srcSectionNames.Except(dstSectionNames).ToList();
        foreach (var removedSectionName in removedSectionNames)
        {
            resultBuilder.AddRemovedSection(removedSectionName);
        }

        return resultBuilder.Build();
    }
}
