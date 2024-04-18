using System.Runtime.CompilerServices;

namespace StaticDataAttribute;

[AttributeUsage(AttributeTargets.Parameter)]
public class OrderAttribute : Attribute
{
    public int Order { get; }

    public OrderAttribute([CallerLineNumber] int order = 0)
    {
        Order = order;
    }
}
