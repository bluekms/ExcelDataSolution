using System.Globalization;
using StaticDataHeaderGenerator.Resources;

namespace StaticDataHeaderGenerator;

internal static class HeaderSeparatorValidator
{
    internal static void Validate(string recordFullName, IReadOnlyList<string> headers, string separator)
    {
        if (string.IsNullOrEmpty(separator))
        {
            return;
        }

        var conflicting = headers
            .Where(h => h.Contains(separator, StringComparison.Ordinal))
            .ToList();

        if (conflicting.Count == 0)
        {
            return;
        }

        throw new InvalidOperationException(string.Format(
            CultureInfo.CurrentCulture,
            Messages.Composite.HeaderContainsSeparator,
            recordFullName,
            separator,
            string.Join(", ", conflicting)));
    }
}
