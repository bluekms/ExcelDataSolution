namespace StaticDataAttribute;

[AttributeUsage(AttributeTargets.Parameter)]
public class SingleColumnContainerAttribute(string separator = ",")
    : Attribute
{
    public string Separator { get; } = separator;
}
