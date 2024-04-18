namespace StaticDataAttribute;

[AttributeUsage(AttributeTargets.Parameter)]
public class ColumnNameAttribute : Attribute
{
    public string Name { get; }

    public ColumnNameAttribute(string name)
    {
        this.Name = name;
    }

    public override bool Match(object? obj)
    {
        return obj is not (List<object> or HashSet<object> or Dictionary<object, object>);
    }
}
