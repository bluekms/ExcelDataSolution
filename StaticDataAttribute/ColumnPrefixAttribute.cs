namespace StaticDataAttribute;

[AttributeUsage(AttributeTargets.Parameter)]
public class ColumnPrefixAttribute : Attribute
{
    public string Prefix { get; }

    public ColumnPrefixAttribute(string prefix)
    {
        Prefix = prefix;
    }

    public override bool Match(object? obj)
    {
        return obj
            is List<object>
            or HashSet<object>
            or Dictionary<object, object>;
    }
}
