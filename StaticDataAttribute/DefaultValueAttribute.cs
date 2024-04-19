namespace StaticDataAttribute;

[AttributeUsage(AttributeTargets.Parameter)]
public class DefaultValueAttribute : Attribute
{
    public string DefaultValue { get; }

    public DefaultValueAttribute(string defaultValue)
    {
        DefaultValue = defaultValue;
    }
}
