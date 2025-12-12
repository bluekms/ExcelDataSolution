namespace Eds;

[AttributeUsage(AttributeTargets.Parameter)]
public class MaxCountAttribute(int maxCount) : Attribute
{
    public int MaxCount { get; } = maxCount;
}
