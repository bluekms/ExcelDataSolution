namespace StaticDataAttribute;

[AttributeUsage(AttributeTargets.Parameter)]
public class NullStringAttribute : Attribute
{
    public string NullString { get; }

    public NullStringAttribute(string nullString)
    {
        NullString = nullString;
    }
}
