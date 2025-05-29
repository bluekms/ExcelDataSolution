namespace StaticDataAttribute;

[AttributeUsage(AttributeTargets.Parameter)]
public class SingleColumnCatalogAttribute(string separator = ",")
    : Attribute
{
    public string Separator { get; } = separator;
}
