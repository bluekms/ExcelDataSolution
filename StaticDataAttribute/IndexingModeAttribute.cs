namespace StaticDataAttribute;

public enum IndexingMode
{
    ZeroBased,
    OneBased,
}

[AttributeUsage(AttributeTargets.Class)]
public class IndexingModeAttribute : Attribute
{
    public IndexingMode Mode { get; }

    public IndexingModeAttribute(IndexingMode mode)
    {
        Mode = mode;
    }
}
