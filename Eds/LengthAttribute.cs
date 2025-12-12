namespace Eds;

[AttributeUsage(AttributeTargets.Parameter)]
public class LengthAttribute(int length)
    : Attribute
{
    public int Length { get; } = length;
}
