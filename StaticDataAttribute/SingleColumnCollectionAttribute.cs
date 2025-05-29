namespace StaticDataAttribute;

[AttributeUsage(AttributeTargets.Parameter)]
public class SingleColumnCollectionAttribute(string separator = ",")
    : Attribute
{
    public string Separator { get; } = separator;
}
