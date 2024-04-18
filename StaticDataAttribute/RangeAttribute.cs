﻿namespace StaticDataAttribute;

public class RangeAttribute : System.ComponentModel.DataAnnotations.RangeAttribute
{
    public RangeAttribute(double minimum, double maximum)
        : base(minimum, maximum)
    {
    }

    public RangeAttribute(int minimum, int maximum)
        : base(minimum, maximum)
    {
    }

    public RangeAttribute(Type type, string minimum, string maximum)
        : base(type, minimum, maximum)
    {
    }
}
