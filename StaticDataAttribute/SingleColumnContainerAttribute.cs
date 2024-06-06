namespace StaticDataAttribute;

[AttributeUsage(AttributeTargets.Parameter)]
public class SingleColumnContainerAttribute : Attribute
{
    public string Separator { get; }

    public SingleColumnContainerAttribute(string separator)
    {
        Separator = separator;
    }
}
