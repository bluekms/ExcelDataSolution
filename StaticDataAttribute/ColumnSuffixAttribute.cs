namespace StaticDataAttribute;

[AttributeUsage(AttributeTargets.Parameter)]
public class ColumnSuffixAttribute : Attribute
{
    public string Suffix { get; }

    public ColumnSuffixAttribute(string suffix)
    {
        Suffix = suffix;
    }

    public override bool Match(object? obj)
    {
        return obj
            is List<object>
            or HashSet<object>
            or Dictionary<object, object>;
    }
}
