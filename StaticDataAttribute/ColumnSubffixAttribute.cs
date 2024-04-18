namespace StaticDataAttribute;

[AttributeUsage(AttributeTargets.Parameter)]
public class ColumnSubffixAttribute : Attribute
{
    public string Suffix { get; }

    public ColumnSubffixAttribute(string suffix)
    {
        this.Suffix = suffix;
    }

    public override bool Match(object? obj)
    {
        return obj
            is List<object>
            or HashSet<object>
            or Dictionary<object, object>;
    }
}
