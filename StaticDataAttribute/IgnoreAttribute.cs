namespace StaticDataAttribute;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Parameter)]
public class IgnoreAttribute : Attribute
{
}
