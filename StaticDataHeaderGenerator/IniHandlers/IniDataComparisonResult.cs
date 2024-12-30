using System.Globalization;
using System.Text;

namespace StaticDataHeaderGenerator.IniHandlers;

public record ModifiedSectionKey(
    string Value1,
    string Value2);

public record ModifiedSection(
    IReadOnlySet<string> AddedKeys,
    IReadOnlySet<string> RemovedKeys,
    IReadOnlyDictionary<string, ModifiedSectionKey> ModifiedKeys);

public record IniDataComparisonResult(
    IReadOnlySet<string> AddedSections,
    IReadOnlySet<string> RemovedSections,
    Dictionary<string, ModifiedSection> ModifiedSections)
{
    public bool IsSame => AddedSections.Count == 0
        && RemovedSections.Count == 0
        && ModifiedSections.Count == 0;

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine();

        if (AddedSections.Count > 0)
        {
            foreach (var section in AddedSections)
            {
                sb.Append("  + ");
                sb.AppendLine(section);
            }
        }

        if (RemovedSections.Count > 0)
        {
            foreach (var section in RemovedSections)
            {
                sb.Append("  - ");
                sb.AppendLine(section);
            }
        }

        if (ModifiedSections.Count > 0)
        {
            sb.AppendLine("Modified sections:");
            foreach (var (section, modifiedSection) in ModifiedSections)
            {
                sb.Append("  ");
                sb.AppendLine(section);
                foreach (var key in modifiedSection.AddedKeys)
                {
                    sb.Append("    + ");
                    sb.AppendLine(key);
                }

                foreach (var key in modifiedSection.RemovedKeys)
                {
                    sb.Append("    - ");
                    sb.AppendLine(key);
                }

                if (modifiedSection.ModifiedKeys.Count > 0)
                {
                    sb.AppendLine("  Modified keys:");
                    foreach (var (key, modifiedKey) in modifiedSection.ModifiedKeys)
                    {
                        sb.Append("    ");
                        sb.AppendLine(CultureInfo.InvariantCulture, $"{key}: {modifiedKey.Value1} -> {modifiedKey.Value2}");
                    }
                }
            }
        }

        return sb.ToString();
    }
}
