namespace StaticDataAttribute;

public class RegularExpressionAttribute : System.ComponentModel.DataAnnotations.RegularExpressionAttribute
{
    public RegularExpressionAttribute(string pattern)
        : base(pattern)
    {
    }
}
