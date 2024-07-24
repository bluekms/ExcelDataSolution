namespace StaticDataAttribute;

public enum IndexingMode
{
    ZeroBased,
    OneBased,
}

/// <summary>
/// This Attribute only affects the output of RecordSchemaFlattener.Flattener()
/// It has no effect on running code.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class IndexingModeAttribute : Attribute
{
    public IndexingMode Mode { get; }

    public IndexingModeAttribute(IndexingMode mode)
    {
        Mode = mode;
    }
}
