namespace Eds;

[AttributeUsage(AttributeTargets.Parameter)]
public class RegularExpressionAttribute(string pattern)
    : System.ComponentModel.DataAnnotations.RegularExpressionAttribute(pattern)
{
}
