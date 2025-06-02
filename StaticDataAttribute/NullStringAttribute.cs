namespace StaticDataAttribute;

[AttributeUsage(AttributeTargets.Parameter)]
public class NullStringAttribute : Attribute
{
    public NullStringAttribute(string nullString)
    {
        NullString = new List<string> { nullString };
    }

    public NullStringAttribute(string[] nullStrings)
    {
        var nullStringList = new HashSet<string>(nullStrings);
        NullString = nullStringList.ToList();
    }

    public IReadOnlyList<string> NullString { get; }
}
