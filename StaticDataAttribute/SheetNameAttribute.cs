namespace StaticDataAttribute;

[AttributeUsage(AttributeTargets.Class)]
public sealed class SheetNameAttribute : Attribute
{
    public string Name { get; }

    public SheetNameAttribute(string name)
    {
        Name = name;
    }
}
