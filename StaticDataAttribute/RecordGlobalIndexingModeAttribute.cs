namespace StaticDataAttribute;

[AttributeUsage(AttributeTargets.Class)]
public class RecordGlobalIndexingModeAttribute : Attribute
{
    public IndexingMode Mode { get; }

    public RecordGlobalIndexingModeAttribute(IndexingMode mode)
    {
        Mode = mode;
    }
}
