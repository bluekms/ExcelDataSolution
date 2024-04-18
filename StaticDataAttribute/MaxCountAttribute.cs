namespace StaticDataAttribute;

[AttributeUsage(AttributeTargets.Parameter)]
public class MaxCountAttribute : Attribute
{
    public int MaxCount { get; }

    public MaxCountAttribute(int maxCount)
    {
        MaxCount = maxCount;
    }
}
