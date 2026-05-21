namespace Sdp.Attributes;

[AttributeUsage(AttributeTargets.Parameter)]
public class ColumnNameAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}
