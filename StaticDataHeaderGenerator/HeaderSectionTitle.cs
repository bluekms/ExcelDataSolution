namespace StaticDataHeaderGenerator;

internal static class HeaderSectionTitle
{
    internal static string Resolve(string separator)
    {
        return separator switch
        {
            "\t" => "### Headers (TSV)",
            "," => "### Headers (CSV)",
            _ => "### Headers",
        };
    }
}
